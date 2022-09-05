/*
 * Active Directory-integrated Android Management
 * Copyright (C) 2022  Manuel Meitinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#nullable enable

using Google;
using Google.Apis.AndroidManagement.v1;
using Google.Apis.AndroidManagement.v1.Data;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using static System.FormattableString;

namespace Aufbauwerk.Tools.Emm
{
    public enum SyncResult
    {
        UpToDate,
        Updated,
        Ignored,
        Deleted,
    }

    public class Enterprise
    {
        public const string DefaultPolicyName = "default";
        private const long TimeOrigin = 621355968000000000;

        private class PrincipalEqualityComparer : IEqualityComparer<Principal>
        {
            public static PrincipalEqualityComparer Instance { get; } = new();

            private PrincipalEqualityComparer() { }

            public bool Equals(Principal x, Principal y) => x.Sid == y.Sid;
            public int GetHashCode(Principal obj) => obj.Sid.GetHashCode();
        }

        public static Enterprise Current => HttpContext.Current.Items["enterprise"] as Enterprise ?? throw new InvalidOperationException("No enterprise registered.");

        public static void Register(string enterpriseName, Action<Enterprise> initialize) => HttpContext.Current.Items["enterprise"] = Helpers.GetCached("enterprise", enterpriseName, enterpriseName =>
        {
            Enterprise enterprise = new(Invariant($"enterprises/{enterpriseName}"));
            initialize(enterprise);
            enterprise._initialized = true;
            return enterprise;
        });

        private readonly HashSet<Principal> _admins = new(PrincipalEqualityComparer.Instance);
        private readonly ConcurrentDictionary<string, Application?> _applicationCache = new();
        private readonly ConcurrentDictionary<(string, string), string> _iframeCache = new();
        private readonly ConcurrentDictionary<string, Policy?> _policyCache = new();
        private bool _initialized = false;
        private bool? _isAdminCache = null;
        private bool? _isUserCache = null;
        private readonly List<string> _policies = new();
        private readonly Dictionary<string, HashSet<Principal>> _policyPrincipals = new();
        private string _provisioningPrefix = "PROVISIONING_";
        private AndroidManagementService? _service = null;
        private bool _useApprove = false;
        private readonly HashSet<Principal> _users = new(PrincipalEqualityComparer.Instance);

        private Enterprise(string name)
        {
            var data = UsingService(service => service.Enterprises.Get(name).Execute());
            Name = name;
            DisplayName = data.EnterpriseDisplayName;
            Color = data.PrimaryColor is null ? null : Invariant($"#{(data.PrimaryColor >> 16) & 0xFF:h2}{(data.PrimaryColor >> 8) & 0xFF:h2}{data.PrimaryColor & 0xFF:h2}");
            Logo = data.Logo?.Url is null ? null : new(data.Logo.Url, UriKind.Absolute);
            Schema = Schema.ForEnterprise(this);
        }

        public string? Color { get; }

        public string DisplayName { get; }

        public bool IsAdmin => CachedCurrentUserIsContainedIn(ref _isAdminCache, _admins);

        public bool IsUser => CachedCurrentUserIsContainedIn(ref _isUserCache, _users);

        public Uri? Logo { get; }

        public string Name { get; }

        public IEnumerable<string> Policies => _policies.Prepend(DefaultPolicyName);

        public Schema Schema { get; }

        public Enterprise Admins(params string[] admins)
        {
            EnsureNotInitialized();
            _admins.UnionWith(admins.Select(EnsurePrincipal));
            return this;
        }

        private JToken BuildUserPolicyAsToken(UserPrincipal user) => Schema.Expand(_policies.Where(policyName => IsContainedIn(user, _policyPrincipals[policyName])).Prepend(DefaultPolicyName).Aggregate(null as JToken, (policy, policyName) => Schema.Merge(policy, FindPolicy(policyName).ToToken())), name => DataBinder.Eval(user, name) as string) ?? new JObject();

        private bool CachedCurrentUserIsContainedIn(ref bool? cache, HashSet<Principal> principals)
        {
            if (!cache.HasValue) { cache = IsContainedIn(Helpers.CurrentUser, principals); }
            return cache.Value;
        }

        public EnrollmentToken CreateEnrollmentToken(string deviceDisplayName, string? policyName = null, bool workProfile = false) => UsingService(service =>
        {
            var token = service.Enterprises.EnrollmentTokens.Create(new()
            {
                AdditionalData = deviceDisplayName,
                AllowPersonalUsage = workProfile ? "PERSONAL_USAGE_ALLOWED" : "PERSONAL_USAGE_DISALLOWED",
                Duration = "900s",
                PolicyName = policyName is null ? null : GetFullPolicyName(EnsureValidName(policyName, nameof(policyName)))
            }, Name).Execute();
            var qrCode = JObject.Parse(token.QrCode);
            qrCode["android.app.extra.PROVISIONING_LOCAL_TIME"] = (DateTime.Now.Ticks - TimeOrigin) / TimeSpan.TicksPerMillisecond;
            foreach (var setting in ConfigurationManager.AppSettings.Keys.OfType<string>().Where(s => s.StartsWith(_provisioningPrefix, StringComparison.Ordinal)))
            {
                qrCode[Invariant($"android.app.extra.PROVISIONING_{setting.Substring(_provisioningPrefix.Length)}")] = ConfigurationManager.AppSettings[setting];
            }
            token.QrCode = qrCode.ToString();
            return token;
        });


        public void DeleteDevice(string deviceName) => UsingService(service => service.Enterprises.Devices.Delete(GetFullDeviceName(EnsureValidName(deviceName, nameof(deviceName)))).Execute());

        public void DeletePolicy(string policyName)
        {
            _policyCache.AddOrUpdate(EnsureValidName(policyName, nameof(policyName)), Delete, (policyName, policy) => policy is null ? null : Delete(policyName));

            Policy? Delete(string policyName)
            {
                UsingServiceAllowNotFound(service => service.Enterprises.Policies.Delete(GetFullPolicyName(policyName)).Execute());
                return null;
            }
        }

        private void EnsureNotInitialized()
        {
            if (_initialized) throw new InvalidOperationException("Enterprise is already initialized.");
        }

        private Principal EnsurePrincipal(string principal) => Helpers.FindPrincipal(principal) ?? throw new ArgumentException($"Principal {principal} not found.");

        private string EnsureValidName(string name, string paramName)
        {
            if (string.IsNullOrEmpty(name)) { throw new ArgumentException($"{paramName} name cannot be empty."); }
            if (name.IndexOf('/') > -1) { throw new ArgumentException($"{paramName} '${name}' must not contain a '/'."); }
            return name;
        }

        private string EnsureValidName(string fullName, string title, Func<string, string> getFullName)
        {
            if (string.IsNullOrEmpty(fullName)) { throw new ArgumentException($"{title} doesn't contain a name."); }
            var name = fullName.Substring(fullName.LastIndexOf('/') + 1);
            if (getFullName(name) != fullName) { throw new ArgumentException($"{title} name '{name}' is invalid."); }
            return name;
        }

        public Application? FindApplication(string packageName) => _applicationCache.GetOrAdd(EnsureValidName(packageName, nameof(packageName)), packageName => UsingServiceAllowNotFound(service => service.Enterprises.Applications.Get(Invariant($"{Name}/applications/{packageName}")).Execute()));

        public Policy? FindPolicy(string policyName) => _policyCache.GetOrAdd(EnsureValidName(policyName, nameof(policyName)), policyName => UsingServiceAllowNotFound(service => service.Enterprises.Policies.Get(GetFullPolicyName(policyName)).Execute()));

        public string GetDeviceName(Device device) => EnsureValidName(device.Name, "Device", GetFullDeviceName);

        private string GetFullDeviceName(string deviceName) => Invariant($"{Name}/devices/{deviceName}");

        private string GetFullPolicyName(string policyName) => Invariant($"{Name}/policies/{policyName}");

        public string GetIFrame(string feature) => _iframeCache.GetOrAdd((EnsureValidName(feature, "Feature"), HttpContext.Current.Request.Url.AbsoluteUri), entry => UsingService(service =>
        {
            var (feature, url) = entry;
            var token = service.Enterprises.WebTokens.Create(new()
            {
                ParentFrameUrl = url,
                EnabledFeatures = new[] { feature }
            }, Name).Execute().Value;
            return Invariant($"https://play.google.com/work/embedded/search?token={token}&iframehomepage={Uri.EscapeDataString(feature)}&mode={(_useApprove ? "APPROVE" : "SELECT")}");
        }));

        public string GetPolicyName(Policy policy) => GetPolicyName(policy.Name);

        private string GetPolicyName(string policyName) => EnsureValidName(policyName, "Policy", GetFullPolicyName);

        private bool IsContainedIn(UserPrincipal user, HashSet<Principal> list) => list.Contains(user) || list.OfType<GroupPrincipal>().Any(user.IsMemberOf);

        public Operation IssueCommand(string deviceName, Command command) => UsingService(service => service.Enterprises.Devices.IssueCommand(command, GetFullDeviceName(EnsureValidName(deviceName, nameof(deviceName)))).Execute());

        private IEnumerable<T> List<T>(Func<AndroidManagementService, string?, (IEnumerable<T>, string?)> action) => UsingService(service =>
        {
            var result = Enumerable.Empty<T>();
            var nextPageToken = null as string;
            do
            {
                IEnumerable<T> elements;
                (elements, nextPageToken) = action(service, nextPageToken);
                if (elements is null) { break; }
                result = result.Concat(elements);
            }
            while (!string.IsNullOrEmpty(nextPageToken));
            return result;
        });

        public IEnumerable<Device> ListDevices() => List((service, nextPageToken) =>
        {
            var request = service.Enterprises.Devices.List(Name);
            request.PageSize = 100;
            request.PageToken = nextPageToken;
            var response = request.Execute();
            return (response.Devices, response.NextPageToken);
        });

        public IEnumerable<Policy> ListPolicies() => List((service, nextPageToken) =>
        {
            var request = service.Enterprises.Policies.List(Name);
            request.PageSize = 100;
            request.PageToken = nextPageToken;
            var response = request.Execute();
            return (response.Policies, response.NextPageToken);
        }).Select(UpdatePolicyCache);

        public void PatchDeviceState(string deviceName, string state) => UsingService(service =>
        {
            var request = service.Enterprises.Devices.Patch(new() { State = state }, GetFullDeviceName(EnsureValidName(deviceName, nameof(deviceName))));
            request.UpdateMask = "state";
            return request.Execute();
        });

        public Policy PatchPolicy(string policyName, Policy policy) => UpdatePolicyCache(UsingService(service => service.Enterprises.Policies.Patch(policy, GetFullPolicyName(EnsureValidName(policyName, nameof(policyName)))).Execute()));

        public Policy PatchUserPolicy(UserPrincipal user)
        {
            try { return PatchPolicy(user.Sid.ToString(), BuildUserPolicyAsToken(user).ToPolicy()); }
            catch (SchemaException e) { throw new GoogleApiException("Schema", e.Message, e); }
        }

        public Enterprise Policy(string policyName, params string[] appliedTo)
        {
            EnsureNotInitialized();
            if (Regex.IsMatch(policyName, @"^[sS](-\d+)+$", RegexOptions.CultureInvariant)) { throw new ArgumentException("Policy names must not be SID-like."); }

            // resolve the groups and add the policy name
            var principals = appliedTo.Select(EnsurePrincipal).ToHashSet(PrincipalEqualityComparer.Instance);
            if (_policyPrincipals.TryGetValue(policyName, out var existingGroups)) { existingGroups.UnionWith(principals); }
            else
            {
                _policies.Add(policyName);
                _policyPrincipals.Add(policyName, principals);
            }
            return this;
        }

        public Enterprise Provisioning(string provisioningPrefix)
        {
            EnsureNotInitialized();
            _provisioningPrefix = provisioningPrefix;
            return this;
        }

        public IEnumerable<(string, SyncResult)> Sync()
        {
            // update all existing policies
            foreach (var policy in ListPolicies())
            {
                // ensure the policy name is a SID
                var policyName = GetPolicyName(policy);
                SecurityIdentifier? sid;
                try { sid = new(policyName); }
                catch (ArgumentException) { sid = null; }
                if (sid is null) { yield return (policyName, SyncResult.Ignored); }
                else
                {
                    // if the user has been deleted, delete the policy
                    var user = Helpers.FindUserBySid(sid);
                    var text = $"{policyName} ({user?.Name ?? "?"})";
                    if (user is null || !IsContainedIn(user, _users))
                    {
                        try { DeletePolicy(policyName); }
                        catch (GoogleApiException e) { text = $"{text} [Failed: {e.GetMessage()}]"; }
                        yield return (text, SyncResult.Deleted);
                    }
                    else
                    {
                        // try to build the updated user policy (might fail if policies are invalid)
                        var oldToken = policy.ToToken();
                        JToken? newToken;
                        try { newToken = BuildUserPolicyAsToken(user); }
                        catch (SchemaException e)
                        {
                            newToken = null;
                            text = $"{text} [Failed: {e.Message}]";
                        }
                        if (newToken is null) { yield return (text, SyncResult.Ignored); }
                        else
                        {
                            // check if the old and new policy are equal, regard failure as not equal
                            bool equals;
                            try { equals = Schema.Equals(oldToken, newToken); }
                            catch (SchemaException e)
                            {
                                equals = false;
                                text = $"{text} [Forced: {e.Message}]";
                            }
                            if (equals) { yield return (text, SyncResult.UpToDate); }
                            else
                            {
                                // update the policy
                                try { PatchPolicy(policyName, newToken.ToPolicy()); }
                                catch (GoogleApiException e) { text = $"{text} [Failed: {e.GetMessage()}]"; }
                                yield return (text, SyncResult.Updated);
                            }
                        }
                    }
                }
            }
        }

        public SecurityIdentifier? TryGetUserSidFromDevice(Device device)
        {
            try { return new(GetPolicyName(device.PolicyName)); }
            catch (ArgumentException) { return null; }
        }

        private Policy UpdatePolicyCache(Policy policy)
        {
            _policyCache[GetPolicyName(policy)] = policy;
            return policy;
        }

        public Enterprise Users(params string[] users)
        {
            EnsureNotInitialized();
            _users.UnionWith(users.Select(EnsurePrincipal));
            return this;
        }

        public Enterprise UseApprove()
        {
            _useApprove = true;
            return this;
        }

        private T UsingService<T>(Func<AndroidManagementService, T> task)
        {
            var service = _service;
            if (service is null)
            {
                service = new(new()
                {
                    ApplicationName = "AufBauWerk EMM",
                    HttpClientInitializer = GoogleCredential.FromFile(HttpContext.Current.Server.MapPath(Helpers.GetSetting("CREDENTIALS"))).CreateScoped(AndroidManagementService.Scope.Androidmanagement),
                });
                _service = service;
            }
            try { return task(service); }
            catch
            {
                _service = null;
                throw;
            }
        }

        private T? UsingServiceAllowNotFound<T>(Func<AndroidManagementService, T> task) => UsingService(service =>
        {
            try { return task(service); }
            catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound) { return default; }
        });
    }
}
