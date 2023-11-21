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

using Google.Apis.AndroidManagement.v1.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using static System.FormattableString;

namespace Aufbauwerk.Tools.Emm
{
    public class SchemaHidden : SchemaString
    {
        public SchemaHidden(string title, string description, string value) : base(title, description, value) { }

        public override Control BuildControl(string input, out Func<string> output)
        {
            output = () => input;
            return Html.Span("uk-text-muted", EnsureValue(input));
        }

        public override string Deserialize(JToken token) => EnsureValue(base.Deserialize(token));

        private string EnsureValue(string value) => value == DefaultValue ? value : throw new SchemaException($"Value '{DefaultValue}' expected for {Title}, got '{value}'.");

        public override JToken? Merge(JToken? first, JToken? second) => DefaultValue;

        public override JToken Serialize(string value) => base.Serialize(EnsureValue(value));
    }

    public class SchemaApplications : SchemaObjectMapHelper
    {
        public SchemaApplications(string title, string description, (string, Schema) key, Enterprise enterprise) : base(title, description, key) => Enterprise = enterprise;

        protected Enterprise Enterprise { get; }

        protected override Schema<JObject> GetSchema(string packageName)
        {
            if (packageName.Contains("/")) { throw new SchemaException($"Invalid package name: {packageName}"); }
            var app = Enterprise.FindApplication(packageName) ?? new Application() { Title = packageName, Description = "Application not found." };
            return new SchemaObject(FromApplication(app), app.Title is null ? packageName : Invariant($"{packageName} [{app.Title}]"), app.Description ?? string.Empty, defaultValue: new(new JProperty(KeyProperty, packageName)));
        }
    }

    public partial class Schema
    {
        public static Schema FromManagedProperty(ManagedProperty managedProperty)
        {
            var title = managedProperty.Title ?? managedProperty.Key ?? string.Empty;
            var description = managedProperty.Description ?? string.Empty;
            return managedProperty.Type switch
            {
                "BOOL" => new SchemaBoolean(title, description, managedProperty.DefaultValue as bool? ?? false),
                "STRING" => new SchemaString(title, description, managedProperty.DefaultValue as string ?? string.Empty),
                "INTEGER" => new SchemaInteger(title, description, managedProperty.DefaultValue as long? ?? 0) { Minimum = null },
                "CHOICE" => FillEntries(managedProperty.DefaultValue is string defaultChoice && IsInEntries(defaultChoice) ? new SchemaEnum(title, description, defaultChoice) : new SchemaEnum(title, description) { { "", "Unspecified." } }),
                "MULTISELECT" => managedProperty.Entries is null ? new SchemaStringArray(title, description, distinct: false) : FillEntries(new SchemaFlags(title, description, managedProperty.DefaultValue is string s && IsInEntries(s) ? new[] { s } : managedProperty.DefaultValue is IEnumerable enumerable ? enumerable.OfType<string>().Where(IsInEntries).ToArray() : new string[0])),
                "BUNDLE" => FillNestedProperties(new SchemaObject(title, description, managedProperty.DefaultValue as JObject)),
                "BUNDLE_ARRAY" => FillNestedProperties(new SchemaObjectArray(title, description)),
                "HIDDEN" => new SchemaHidden(title, description, managedProperty.DefaultValue as string ?? string.Empty),
                _ => throw new ArgumentException($"Type '{managedProperty.Type}' of managed property {title} is invalid."),
            };

            bool IsInEntries(string s) => managedProperty.Entries?.Any(entry => entry.Value == s) ?? false;
            T FillEntries<T>(T schema) where T : ISchemaInitializable<string> => schema.SaveAdd(managedProperty.Entries, entry => entry.Value, entry => entry.Name ?? string.Empty);
            T FillNestedProperties<T>(T schema) where T : ISchemaInitializable<Schema> => schema.SaveAdd(managedProperty.NestedProperties, nestedProperty => nestedProperty.Key, FromManagedProperty);
        }

        public static SchemaObject FromApplication(Application application) => new(application.Title, application.Description)
        {
            { "disabled", new SchemaBoolean("Disabled", "Whether the app is disabled. When disabled, the app data is still preserved.") },
            { "installType", new SchemaEnum("Install Type", "The type of installation to perform.") {
                { "INSTALL_TYPE_UNSPECIFIED", "Unspecified. Defaults to AVAILABLE." },
                { "PREINSTALLED", "The app is automatically installed and can be removed by the user." },
                { "FORCE_INSTALLED", "The app is automatically installed and can't be removed by the user." },
                { "BLOCKED", "The app is blocked and can't be installed. If the app was installed under a previous policy, it will be uninstalled." },
                { "AVAILABLE", "The app is available to install." },
                { "REQUIRED_FOR_SETUP", "The app is automatically installed and can't be removed by the user and will prevent setup from completion until installation is complete." },
                { "KIOSK", "The app is automatically installed in kiosk mode: it's set as the preferred home intent and whitelisted for lock task mode. Device setup won't complete until the app is installed. After installation, users won't be able to remove the app. You can only set this installType for one app per policy. When this is present in the policy, status bar will be automatically disabled." },
            } },
            { "autoUpdateMode", new SchemaEnum("Auto Update Mode", "Controls the auto-update mode for the app.") {
                { "AUTO_UPDATE_MODE_UNSPECIFIED", "Unspecified. Defaults to AUTO_UPDATE_DEFAULT." },
                { "AUTO_UPDATE_DEFAULT", "The app is automatically updated with low priority to minimize the impact on the user. The app is updated when all of the following constraints are met: The device is not actively used. The device is connected to an unmetered network. The device is charging. The device is notified about a new update within 24 hours after it is published by the developer, after which the app is updated the next time the constraints above are met." },
                { "AUTO_UPDATE_POSTPONED", "The app is not automatically updated for a maximum of 90 days after the app becomes out of date. 90 days after the app becomes out of date, the latest available version is installed automatically with low priority (see AUTO_UPDATE_DEFAULT). After the app is updated it is not automatically updated again until 90 days after it becomes out of date again. The user can still manually update the app from the Play Store at any time." },
                { "AUTO_UPDATE_HIGH_PRIORITY", "The app is updated as soon as possible. No constraints are applied. The device is notified immediately about a new update after it becomes available." },
            } },
            { "managedConfiguration", new SchemaObject("Managed Configuration", "Managed configuration applied to the app.").SaveAdd(application.ManagedProperties, managedProperty => managedProperty.Key, FromManagedProperty) },
            { "defaultPermissionPolicy", new SchemaEnum("Default Permission Policy", "The default policy for all permissions requested by the app. If specified, this overrides the policy-level default_permission_policy which applies to all apps. It does not override the permission_grants which applies to all apps.") {
                { "PERMISSION_POLICY_UNSPECIFIED", "Policy not specified. If no policy is specified for a permission at any level, then the PROMPT behavior is used by default." },
                { "PROMPT", "Prompt the user to grant a permission." },
                { "GRANT", "Automatically grant a permission." },
                { "DENY", "Automatically deny a permission." },
            } },
            { "permissionGrants", new SchemaObjectMap("Permission Grants", "Explicit permission grants or denials for the app. These values override the defaultPermissionPolicy and permissionGrants which apply to all apps.", key: ("permission", new SchemaEnum("Permission", "The Android permission or group, e.g. android.permission.READ_CALENDAR or android.permission_group.CALENDAR.")
                {
                    { "", "Unspecified." }
                }.SaveAdd(application.Permissions, permission => permission.PermissionId, permission => permission.Description ?? string.Empty)), editAllProperties: true)
                {
                    { "policy", new SchemaEnum("Policy", "The policy for granting the permission.") {
                        { "PERMISSION_POLICY_UNSPECIFIED", "Policy not specified. If no policy is specified for a permission at any level, then the PROMPT behavior is used by default." },
                        { "PROMPT", "Prompt the user to grant a permission." },
                        { "GRANT", "Automatically grant a permission." },
                        { "DENY", "Automatically deny a permission." },
                    } },
                }
            },
            { "minimumVersionCode", new SchemaInteger("Minimum Version Code", "The minimum version of the app that runs on the device. If set, the device attempts to update the app to at least this version code. If the app is not up-to-date, the device will contain a NonComplianceDetail with non_compliance_reason set to APP_NOT_UPDATED. The app must already be published to Google Play with a version code greater than or equal to this value. At most 20 apps may specify a minimum version code per policy.") },
            { "delegatedScopes", new SchemaFlags("Delegated Scopes", "The scopes delegated to the app from Android Device Policy.") {
                { "DELEGATED_SCOPE_UNSPECIFIED", "No delegation scope specified." },
                { "CERT_INSTALL", "Grants access to certificate installation and management." },
                { "MANAGED_CONFIGURATIONS", "Grants access to managed configurations management." },
                { "BLOCK_UNINSTALL", "Grants access to blocking uninstallation." },
                { "PERMISSION_GRANT", "Grants access to permission policy and permission grant state." },
                { "PACKAGE_ACCESS", "Grants access to package access state." },
                { "ENABLE_SYSTEM_APP", "Grants access for enabling system apps." },
            } },
            { "accessibleTrackIds", new SchemaFlags("Accessible Track Ids", "List of the app's track IDs that a device belonging to the enterprise can access. If the list contains multiple track IDs, devices receive the latest version among all accessible tracks. If the list contains no track IDs, devices only have access to the app's production track. More details about each track are available in AppTrackInfo.").SaveAdd(application.AppTracks, appTrack => appTrack.TrackId, appTrack => appTrack.TrackAlias ?? string.Empty) },
            { "connectedWorkAndPersonalApp", new SchemaEnum("Connected Work And Personal App", "Controls whether the app can communicate with itself across a device's work and personal profiles, subject to user consent.") {
                { "CONNECTED_WORK_AND_PERSONAL_APP_UNSPECIFIED", "Unspecified. Defaults to CONNECTED_WORK_AND_PERSONAL_APPS_DISALLOWED." },
                { "CONNECTED_WORK_AND_PERSONAL_APP_DISALLOWED", "Default. Prevents the app from communicating cross-profile." },
                { "CONNECTED_WORK_AND_PERSONAL_APP_ALLOWED", "Allows the app to communicate across profiles after receiving user consent." },
            } },
            { "alwaysOnVpnLockdownExemption", new SchemaEnum("Always-On VPN Lockdown Exemption", "Specifies whether the app is allowed networking when the VPN is not connected and alwaysOnVpnPackage.lockdownEnabled is enabled.") {
                { "ALWAYS_ON_VPN_LOCKDOWN_EXEMPTION_UNSPECIFIED", "Unspecified. Defaults to VPN_LOCKDOWN_ENFORCED." },
                { "VPN_LOCKDOWN_ENFORCED", "The app respects the always-on VPN lockdown setting." },
                { "VPN_LOCKDOWN_EXEMPTION", "The app is exempt from the always-on VPN lockdown setting." },
            } },
            { "workProfileWidgets", new SchemaEnum("Work Profile Widgets", "Specifies whether the app installed in the work profile is allowed to add widgets to the home screen.") {
                { "WORK_PROFILE_WIDGETS_UNSPECIFIED", "Unspecified. Defaults to workProfileWidgetsDefault." },
                { "WORK_PROFILE_WIDGETS_ALLOWED", "Work profile widgets are allowed. This means the application will be able to add widgets to the home screen." },
                { "WORK_PROFILE_WIDGETS_DISALLOWED", "Work profile widgets are disallowed. This means the application will not be able to add widgets to the home screen." },
            } },
        };
    }
}
