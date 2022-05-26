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
using Google.Apis.AndroidManagement.v1.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using static System.FormattableString;

namespace Aufbauwerk.Tools.Emm
{
    public class NamingContainer : Control, INamingContainer
    {
        public static Control Wrap(Control control)
        {
            NamingContainer result = new();
            result.Controls.Add(control);
            return result;
        }

        private NamingContainer() { }
    }

    public class ValueContainer : Control, INamingContainer
    {
        public static Control Build<T>(T initialValue, Func<T, Action<T>, Control> builder) => Build(initialValue, (value, setValue) => Enumerable.Repeat(builder(value, setValue), 1));

        public static Control Build<T>(T initialValue, Func<T, Action<T>, IEnumerable<Control>> builder)
        {
            ValueContainer container = new(container => builder((T)container.ViewState[ValueName], value =>
            {
                container.ViewState[ValueName] = value;
                container.Controls.Clear();
                container.Build();
            }));
            container.ViewState[ValueName] = initialValue;
            return container;
        }

        private const string ValueName = "value";

        private readonly Func<ValueContainer, IEnumerable<Control>> _builder;

        private ValueContainer(Func<ValueContainer, IEnumerable<Control>> builder) => _builder = builder;

        private void Build()
        {
            foreach (var control in _builder(this)) { Controls.Add(control); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Build();
        }
    }

    public class Html : Control
    {
        public static Html Tag(string tagName) => new(tagName);

        public static Html Span(string text) => Tag("span").Text(text);

        public static Html Span(string cssClass, string text) => Span(text).Class(cssClass);

        public static Html Div(params Control[] controls) => Tag("div").Children(controls);

        public static Html Div(string cssClass, params Control[] controls) => Div(controls).Class(cssClass);

        private readonly Dictionary<string, string> _attributes = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _tagName;
        private string? _text;

        private Html(string tagName) => _tagName = tagName;

        public Html Class(string className) => Attribute("class", className);

        public Html Title(string title) => Attribute("title", title);

        public Html Attribute(string name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        public Html Text(string text)
        {
            _text = text;
            return this;
        }

        public Html Children(IEnumerable<Control> children)
        {
            foreach (var child in children) { Controls.Add(child); }
            return this;
        }

        public Html Children(params Control[] children) => Children((IEnumerable<Control>)children);

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteBeginTag(_tagName);
            foreach (var attribute in _attributes) { writer.WriteAttribute(attribute.Key, attribute.Value, fEncode: true); }
            writer.Write(">");
            if (_text is not null) { writer.WriteEncodedText(_text); }
            RenderChildren(writer);
            writer.WriteEndTag(_tagName);
        }
    }

    public static class Helpers
    {
        private static readonly PrincipalContext _context = new(ContextType.Domain, GetSetting("DOMAIN"));

        public static UserPrincipal CurrentUser => FindUserBySid(HttpContext.Current.Request.LogonUserIdentity.User) ?? throw new UnauthorizedAccessException($"The current user {HttpContext.Current.Request.LogonUserIdentity.Name} has not been found.");

        public static Principal? FindPrincipal(string identityValue) => GetCached("principal", identityValue, identityValue => Principal.FindByIdentity(_context, identityValue));

        public static UserPrincipal? FindUserBySid(SecurityIdentifier sid) => GetCached("user", sid.ToString(), sid => UserPrincipal.FindByIdentity(_context, IdentityType.Sid, sid));

        public static T GetCached<T>(string category, string name, Func<string, T> builder)
        {
            var key = Invariant($"{category}/${name}");
            if (HttpContext.Current.Session[key] is not T result)
            {
                result = builder(name);
                HttpContext.Current.Session[key] = result;
            }
            return result;
        }

        public static string GetSetting(string name) => ConfigurationManager.AppSettings[name] ?? throw new ConfigurationErrorsException($"web.conf is missing\n<configuration>\n  <appSettings>\n    <add key=\"{name}\" value=\"...\" />\n  </appSettings>\n<configuration>");
    }

    public static class Extensions
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault(new() { NullValueHandling = NullValueHandling.Ignore });

        public static void Alert(this Page page, string message) => ScriptManager.RegisterStartupScript
        (
            page: page,
            type: page.GetType(),
            key: "alert",
            script: Invariant($"alert({HttpUtility.JavaScriptStringEncode(message, addDoubleQuotes: true)})"),
            addScriptTags: true
        );

        public static JArray AsArray(this JToken token) => token is JArray array ? array : throw token.Expected(JTokenType.Array);

        public static bool AsBoolean(this JToken token) => token is JValue value && value.Value is bool b ? b : throw token.Expected(JTokenType.Boolean);

        public static long AsInteger(this JToken token) => token is JValue value ? (value.Value switch { sbyte sb => sb, byte b => b, short s => s, ushort us => us, int i => i, uint ui => ui, long l => l, ulong ul => (long)ul, _ => throw value.Expected(JTokenType.Integer) }) : throw token.Expected(JTokenType.Integer);

        public static JObject AsObject(this JToken token) => token is JObject obj ? obj : throw token.Expected(JTokenType.Object);

        public static string AsString(this JToken token) => token.AsStringNoThrow() ?? throw token.Expected(JTokenType.String);

        public static string? AsStringNoThrow(this JToken token) => token is JValue value && value.Value is string s ? s : null;

        private static SchemaException Expected(this JToken token, JTokenType expected) => new($"{expected} expected but got {token.Type}.");

        public static string GetMessage(this GoogleApiException e) => e.Error?.Message ?? e.Message;

        public static T ID<T>(this T control, string id) where T : Control
        {
            control.ID = id;
            return control;
        }

        public static T SaveAdd<T, U, V>(this T schema, IEnumerable<U?>? collection, Func<U, string?> name, Func<U, V> value) where T : ISchemaInitializable<V>
        {
            if (collection is not null)
            {
                foreach (var entry in collection)
                {
                    if (entry is not null)
                    {
                        try { schema.Add(name(entry) ?? throw new ArgumentException("No name specified."), value(entry)); }
                        catch (ArgumentException) { }
                    }
                }
            }
            return schema;
        }

        public static Policy ToPolicy(this JToken token) => token?.ToObject<Policy>(_serializer) ?? new();

        public static JToken? ToToken(this Policy? policy) => policy is null ? null : JToken.FromObject(policy, _serializer);

        public static int Xor(this IEnumerable<int> values) => values.Aggregate(0, (a, b) => a ^ b);
    }
}
