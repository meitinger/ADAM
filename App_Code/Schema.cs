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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.FormattableString;

namespace Aufbauwerk.Tools.Emm
{
    public class SchemaException : Exception
    {
        public SchemaException(string message) : base(message) { }
    }

    public delegate bool SchemaResult<T>(out T value);

    public abstract partial class Schema : IEqualityComparer<JToken?>
    {
        public Schema(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Description { get; }

        public string Title { get; }

        public abstract Control BuildControl(JToken? input, out SchemaResult<JToken?> output, Action? delete = null, bool allowView = false, params Control[] customEditButtons);

        public abstract bool Equals(JToken? first, JToken? second);

        public abstract JToken? Expand(JToken? token, Func<string, string?> lookup);

        public abstract string Format(JToken? token);

        public abstract int GetHashCode(JToken? token);

        public abstract JToken? Merge(JToken? first, JToken? second);
    }

    public abstract class Schema<T> : Schema, IEqualityComparer<T>
    {
        public Schema(string title, string description) : base(title, description) { }

        public abstract T DefaultValue { get; }

        public override Control BuildControl(JToken? input, out SchemaResult<JToken?> output, Action? delete = null, bool allowView = false, params Control[] customEditButtons)
        {
            // forward the output to a local variable
            var token = input;
            Func<bool>? save = null;
            output = (out JToken? output) =>
            {
                var result = save?.Invoke() ?? true;
                output = token;
                return result;
            };

            return ValueContainer.Build((input?.ToString(Formatting.None), !allowView), (state, setState) =>
            {
                // decode the view state
                var (rawValue, isEditing) = state;
                token = rawValue is null ? null : JToken.Parse(rawValue);
                save = null;

                // common delete button
                Button deleteButton = new()
                {
                    ID = "delete",
                    Text = "Delete",
                    CssClass = "uk-button uk-button-danger",
                    UseSubmitBehavior = false,
                    Enabled = token is not null,
                };
                deleteButton.Click += (s, e) =>
                {
                    setState((null, !allowView));
                    delete?.Invoke();
                };

                if (isEditing)
                {
                    // turn the token into a .NET type
                    T value;
                    if (token is null) { value = DefaultValue; }
                    else
                    {
                        try { value = Deserialize(token); }
                        catch (SchemaException ex)
                        {
                            // on error allow the value to be reseted
                            return
                                Html.Div("uk-card uk-card-small uk-card-default",
                                    Html.Div("uk-card-header",
                                        BuildTitle(),
                                        BuildDescription()),
                                    Html.Div("uk-card-body uk-alert-warning",
                                        Html.Tag("p").Class("uk-text-large").Text("The existing value is invalid."),
                                        Html.Tag("p").Text(ex.Message),
                                        Html.Div("uk-text-center",
                                            deleteButton)));
                        }
                    }

                    // build the card and control
                    Panel card = new() { CssClass = "uk-card uk-card-small uk-card-default" };
                    var control = BuildControl(value, out var result).ID("control");
                    save = () =>
                    {
                        // serialize and store the new value
                        try
                        {
                            token = Serialize(result());
                            return true;
                        }
                        catch (SchemaException e)
                        {
                            card.Controls.Add(Html.Div("uk-card-footer uk-alert-danger", Html.Span(e.Message)));
                            return false;
                        }
                    };

                    // build the header buttons
                    var buttons = Html.Div("uk-margin-left uk-button-group uk-width-auto", customEditButtons);
                    if (customEditButtons.Length == 0 && allowView)
                    {
                        Button okButton = new()
                        {
                            ID = "ok",
                            Text = "OK",
                            CssClass = "uk-button uk-button-primary",
                            UseSubmitBehavior = false,
                        };
                        okButton.Click += (s, e) => { if (save()) { setState((token?.ToString(Formatting.None), false)); } };
                        buttons.Children(okButton);
                        Button cancelButton = new()
                        {
                            ID = "cancel",
                            Text = "Cancel",
                            CssClass = "uk-button uk-button-danger",
                            UseSubmitBehavior = false,
                        };
                        cancelButton.Click += (s, e) => setState((rawValue, false));
                        buttons.Children(cancelButton);
                    }
                    card.DefaultButton = buttons.Controls.OfType<Button>().FirstOrDefault(button => button.CssClass.Contains("uk-button-primary"))?.ID;

                    card.Controls.Add(
                        Html.Div("uk-card-header uk-flex uk-flex-top",
                            Html.Div("uk-width-expand",
                                BuildTitle(),
                                BuildDescription()
                            ),
                            buttons));
                    card.Controls.Add(
                        Html.Div("uk-card-body",
                            control));
                    return card;

                    Control BuildTitle() => Html.Tag("h3").Class("uk-card-title uk-margin-remove-bottom").Text(Title);
                    Control BuildDescription() => Html.Tag("p").Class("uk-text-meta uk-margin-remove").Text(Description);
                }
                else
                {
                    // build the view controls
                    var text = Format(token);
                    Button editButton = new()
                    {
                        ID = "edit",
                        Text = "Edit",
                        CssClass = "uk-button uk-button-small",
                        UseSubmitBehavior = false,
                    };
                    editButton.Click += (s, e) => setState((rawValue, true));
                    deleteButton.CssClass += " uk-button-small";
                    return
                        Html.Div("uk-flex uk-flex-middle",
                            Html.Div("uk-margin-right uk-width-2-5 uk-text-truncate",
                                Html.Span(Title).Title(Description)
                            ),
                            Html.Div("uk-width-expand uk-text-truncate",
                                Html.Span(token is null ? "" : "uk-text-bolder", text).Title(text)
                            ),
                            Html.Div("uk-margin-left uk-button-group uk-width-auto",
                                editButton,
                                deleteButton));
                }
            });
        }

        public abstract Control BuildControl(T input, out Func<T> output);

        public abstract T Deserialize(JToken token);

        public override bool Equals(JToken? first, JToken? second) => first is null && second is null || Equals(first is null ? DefaultValue : Deserialize(first), second is null ? DefaultValue : Deserialize(second));

        public abstract bool Equals(T first, T second);

        public override JToken? Expand(JToken? token, Func<string, string?> lookup) => token is null ? null : Serialize(Expand(Deserialize(token), lookup));

        public abstract T Expand(T value, Func<string, string?> lookup);

        public override string Format(JToken? token)
        {
            T value;
            try { value = token is null ? DefaultValue : Deserialize(token); }
            catch (SchemaException e) { return e.Message; }
            return Format(value);
        }

        public abstract string Format(T value);

        public override int GetHashCode(JToken? token) => token is null ? 0 : GetHashCode(Deserialize(token));

        public override JToken? Merge(JToken? first, JToken? second) => first is null && second is null ? null : Serialize(Merge(first is null ? DefaultValue : Deserialize(first), second is null ? DefaultValue : Deserialize(second)));

        public abstract T Merge(T first, T second);

        public abstract int GetHashCode(T value);

        public abstract JToken Serialize(T value);
    }

    public abstract class SchemaValue<T> : Schema<T> where T : IEquatable<T>
    {
        public SchemaValue(string title, string description) : base(title, description) { }

        public override bool Equals(T first, T second) => first.Equals(second);

        public override T Expand(T value, Func<string, string?> lookup) => value;

        public override int GetHashCode(T value) => value.GetHashCode();

        public override T Merge(T first, T second) => second;
    }

    public class SchemaInvalid<T> : Schema<T>
    {
        public SchemaInvalid(string title, SchemaException exception) : base(title, exception.Message) => Exception = exception;

        public override T DefaultValue => throw Exception;

        private SchemaException Exception { get; }

        public override Control BuildControl(T input, out Func<T> output) => throw new InvalidOperationException();
        public override T Deserialize(JToken token) => throw Exception;
        public override bool Equals(T first, T second) => throw Exception;
        public override T Expand(T value, Func<string, string?> lookup) => throw Exception;
        public override string Format(T value) => Exception.Message;
        public override int GetHashCode(T value) => throw Exception;
        public override T Merge(T first, T second) => throw Exception;
        public override JToken Serialize(T value) => JValue.CreateNull();
    }

    public abstract class SchemaArray<T> : Schema<T[]>
    {
        public SchemaArray(string title, string description) : base(title, description) { }

        public override T[] DefaultValue { get; } = new T[0];

        public override Control BuildControl(T[] input, out Func<T[]> output)
        {
            // use stored callbacks to gather the result
            Dictionary<int, SchemaResult<T>> itemResults = new();
            Dictionary<int, Control> newItemControls = new();
            output = () =>
            {
                List<T> result = new();
                foreach (var itemResult in itemResults.Values) { if (itemResult(out var value)) { result.Add(value); } };
                var failed = itemResults.Count - result.Count;
                if (failed > 0) { throw new SchemaException($"{failed} items have issues."); }
                return result.ToArray();
            };

            return NamingContainer.Wrap(
                Html.Tag("ul").Class("uk-list uk-list-divider")
                    .Children(input.Select(BuildItemWithID))
                    .Children(ValueContainer.Build(Array.AsReadOnly(new string[0]), (list, setList) => list
                        .Select((rawItem, index) =>
                        {
                            index += input.Length;
                            if (newItemControls.TryGetValue(index, out var control)) { return control; }
                            else
                            {
                                var token = JToken.Parse(rawItem);
                                control = BuildItemWithID(GetSchema(token, index).Deserialize(token), index);
                                newItemControls.Add(index, control);
                                return control;
                            }
                        })
                        .Append(Html.Tag("li")
                            .Children(NewTemplate(item => setList(Array.AsReadOnly(list.Append(GetSchema(item, input.Length + list.Count).Serialize(item).ToString(Formatting.None)).ToArray()))).ID(Invariant($"new_item_{input.Length + list.Count}")))))));

            Control BuildItemWithID(T item, int index) => ValueContainer.Build(false, (deleted, setDeleted) =>
            {
                var result = Enumerable.Empty<Control>();
                if (!deleted)
                {
                    result = result.Append(Html.Tag("li").Children(ItemTemplate(item, index, out var updateItem, removeItem: () =>
                    {
                        itemResults.Remove(index);
                        setDeleted(true);
                    }).ID("template")));
                    itemResults.Add(index, updateItem);
                }
                return result;
            }).ID(Invariant($"item_{index}"));
        }

        public override T[] Deserialize(JToken token) => token.AsArray().Select((token, index) => GetSchema(token, index).Deserialize(token)).ToArray();

        public override T[] Expand(T[] values, Func<string, string?> lookup) => values.Select((value, index) => GetSchema(value, index).Expand(value, lookup)).ToArray();

        public override string Format(T[] values) => $"[{string.Join(", ", values.Select((value, index) => GetSchema(value, index).Format(value)))}]";

        protected abstract Schema<T> GetSchema(JToken token, int index);

        protected abstract Schema<T> GetSchema(T item, int index);

        protected virtual Control ItemTemplate(T item, int index, out SchemaResult<T> updateItem, Action removeItem)
        {
            Schema<T> schema;
            try { schema = GetSchema(item, index); }
            catch (SchemaException e) { schema = new SchemaInvalid<T>($"Invalid Item {index + 1}", e); }
            var control = schema.BuildControl
            (
                input: schema.Serialize(item),
                output: out var result,
                delete: removeItem,
                allowView: true
            );
            updateItem = (out T value) =>
            {
                if (result(out var token))
                {
                    value = token is null ? schema.DefaultValue : schema.Deserialize(token);
                    return true;
                }
                else
                {
                    value = schema.DefaultValue;
                    return false;
                }
            };
            return control;
        }

        protected virtual Control NewTemplate(Action<T> addItem) => new();

        public override JToken Serialize(T[] values) => new JArray(values.Select((value, index) => GetSchema(value, index).Serialize(value)));
    }

    public abstract class SchemaValueArray<U, T> : SchemaArray<T> where U : Schema<T>
    {
        public SchemaValueArray(U inner, bool distinct) : base(inner.Title, inner.Description)
        {
            Inner = inner;
            Distinct = distinct;
        }

        protected bool Distinct { get; }

        protected U Inner { get; }

        public override Control BuildControl(T[] input, out Func<T[]> output)
        {
            // for distinct arrays ensure no duplicate items are returned
            if (Distinct)
            {
                var control = base.BuildControl(input, out var outputInner);
                output = () =>
                {
                    var result = outputInner();
                    if (result.Length != result.Distinct(Inner).Count()) { throw new SchemaException("Duplicate items are not allowed."); }
                    return result;
                };
                return control;
            }
            else { return base.BuildControl(input, out output); }
        }

        private IEnumerable<T> EnsureDistinct(IEnumerable<T> values) => Distinct ? values.Distinct(Inner) : values;

        public override int GetHashCode(T[] values) => EnsureDistinct(values).Select(Inner.GetHashCode).Xor();

        protected override Schema<T> GetSchema(JToken token, int index) => Inner;

        protected override Schema<T> GetSchema(T item, int index) => Inner;

        public override bool Equals(T[] first, T[] second) => Distinct ? first.ToHashSet(Inner).SetEquals(second.ToHashSet(Inner)) : first.SequenceEqual(second, Inner);

        public override T[] Merge(T[] first, T[] second) => EnsureDistinct(first.Concat(second)).ToArray();
    }

    public interface ISchemaInitializable<T> : IEnumerable<(string, T)>
    {
        void Add(string name, T value);
    }

    public class SchemaEntries<T> : IEnumerable<string>
    {
        private readonly Dictionary<string, T> _entries = new();
        private readonly List<string> _order = new();

        public int Count => _entries.Count;

        public T this[string entry] => _entries[entry];

        public string this[int index] => _order[index];

        public virtual void Add(string name, T value)
        {
            _entries.Add(name, value);
            _order.Add(name);
        }

        public IEnumerable<(string, T)> Entries => _order.Select(entry => (entry, _entries[entry]));

        public int IndexOf(string entry) => _order.IndexOf(entry);

        public IEnumerator<string> GetEnumerator() => _order.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class SchemaObject : Schema<JObject>, ISchemaInitializable<Schema>
    {
        public SchemaObject(string title, string description, JObject? defaultValue = null, bool editAllProperties = false) : this(title, description, defaultValue ?? new(), editAllProperties, new()) { }

        public SchemaObject(SchemaObject template, string title, string description, JObject? defaultValue = null) : this(title, description, defaultValue ?? template.DefaultValue, template.EditAllProperties, template.Properties) { }

        private SchemaObject(string title, string description, JObject defaultValue, bool editAllProperties, SchemaEntries<Schema> properties) : base(title, description)
        {
            DefaultValue = defaultValue;
            EditAllProperties = editAllProperties;
            Properties = properties;
        }

        public override JObject DefaultValue { get; }

        protected bool EditAllProperties { get; }

        protected SchemaEntries<Schema> Properties { get; }

        public virtual void Add(string name, Schema value) => Properties.Add(name, value);

        public override Control BuildControl(JObject input, out Func<JObject> output)
        {
            // use stored callbacks to gather the result
            Dictionary<string, SchemaResult<JToken?>> propertyResults = new();
            output = () =>
            {
                // check for errors and build the resulting JObjectts
                HashSet<string> propertyTitlesWithErrors = new();
                JObject result = (JObject)DefaultValue.DeepClone();
                foreach (var property in Properties)
                {
                    if (propertyResults[property](out var value))
                    {
                        if (value is not null) { result.Add(property, value); }
                    }
                    else { propertyTitlesWithErrors.Add(Properties[property].Title); }
                }
                if (propertyTitlesWithErrors.Count > 0) { throw new SchemaException($"The following properties contain errors: {string.Join(", ", propertyTitlesWithErrors)}"); }
                return result;
            };

            // create a control for each property
            return NamingContainer.Wrap(
                Html.Tag("ul").Class("uk-list uk-list-divider").Children(Properties.Select(property =>
                {
                    var control = Properties[property].BuildControl
                    (
                        input: input[property],
                        output: out var result,
                        allowView: !EditAllProperties
                    ).ID(Invariant($"property_{property}"));
                    propertyResults.Add(property, result);
                    return Html.Tag("li").Children(control);
                })));
        }

        public override JObject Deserialize(JToken token) => token.AsObject();

        public override bool Equals(JObject first, JObject second) => Properties.All(property => Properties[property].Equals(first[property], second[property]));

        public override JObject Expand(JObject value, Func<string, string?> lookup)
        {
            JObject result = (JObject)DefaultValue.DeepClone();
            foreach (var property in Properties)
            {
                var token = Properties[property].Expand(value[property], lookup);
                if (token is not null) { result.Add(property, token); }
            }
            return result;
        }

        public override string Format(JObject value) => $"{{{string.Join(", ", Properties.Where(value.ContainsKey))}}}";

        public IEnumerator<(string, Schema)> GetEnumerator() => Properties.Entries.GetEnumerator();

        public override int GetHashCode(JObject value) => Properties.Select(property => Properties[property].GetHashCode(value[property])).Xor();

        public override JObject Merge(JObject first, JObject second)
        {
            // merge each property recursively
            JObject result = (JObject)DefaultValue.DeepClone();
            foreach (var property in Properties)
            {
                var value = Properties[property].Merge(first[property], second[property]);
                if (value is not null) { result.Add(property, value); }
            }
            return result;
        }

        public override JToken Serialize(JObject value) => value;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class SchemaObjectArray : SchemaValueArray<SchemaObject, JObject>, ISchemaInitializable<Schema>
    {
        public SchemaObjectArray(string title, string description, bool distinct = false, JObject? defaultValue = null, bool editAllProperties = false) : base(new(title, description, defaultValue, editAllProperties), distinct) { }

        public void Add(string name, Schema value) => Inner.Add(name, value);

        public override string Format(JObject[] value) => value.Length switch
        {
            0 => "[]",
            1 => "[object]",
            _ => $"[{value.Length} objects]",
        };

        public IEnumerator<(string, Schema)> GetEnumerator() => Inner.GetEnumerator();

        protected override Schema<JObject> GetSchema(JToken token, int index) => GetSchema(token.AsObject(), index);

        protected override Schema<JObject> GetSchema(JObject value, int index) => new SchemaObject(Inner, $"Item {index + 1}", string.Empty);

        protected override Control NewTemplate(Action<JObject> addItem)
        {
            Button addButton = new()
            {
                Text = $"New {Inner.Title}",
                CssClass = "uk-button uk-button-primary",
                UseSubmitBehavior = false,
            };
            addButton.Click += (s, e) => addItem(new());
            return addButton;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public abstract class SchemaObjectMapHelper : SchemaArray<JObject>
    {
        private readonly ConcurrentDictionary<string, Schema<JObject>> _schemaCache = new();

        public SchemaObjectMapHelper(string title, string description, (string, Schema) key, string? defaultKeyValue = null) : base(title, description)
        {
            (KeyProperty, KeySchema) = key;
            DefaultKeyValue = string.IsNullOrEmpty(defaultKeyValue) ? null as JToken : defaultKeyValue;
        }

        protected JToken? DefaultKeyValue { get; }

        protected string KeyProperty { get; }

        protected Schema KeySchema { get; }

        public override Control BuildControl(JObject[] input, out Func<JObject[]> output)
        {
            var control = base.BuildControl(input, out var outputInner);
            output = () => BuildKeyMapping(outputInner()).Values.ToArray();
            return control;
        }

        private IDictionary<string, JObject> BuildKeyMapping(JObject[] array)
        {
            // ensure that each item has an unique key
            SortedDictionary<string, JObject> result = new();
            int itemsWithoutKey = 0;
            HashSet<string> duplicateKeys = new();
            foreach (var item in array)
            {
                var rawKey = item[KeyProperty] ?? DefaultKeyValue;
                if (rawKey is null) { itemsWithoutKey++; }
                else
                {
                    var key = rawKey.AsString();
                    if (key.Length == 0) { itemsWithoutKey++; }
                    else if (result.ContainsKey(key)) { duplicateKeys.Add(key); }
                    else { result.Add(key, item); }
                }
            }
            if (itemsWithoutKey > 0) { throw new SchemaException($"{itemsWithoutKey} items have no {KeySchema.Title} value."); }
            if (duplicateKeys.Count > 0) { throw new SchemaException($"The following {KeySchema.Title} values occur more than once: {string.Join(", ", duplicateKeys)}"); }
            return result;
        }

        public override bool Equals(JObject[] first, JObject[] second)
        {
            // compare elements with the same key
            var keyedFirst = BuildKeyMapping(first);
            var keyedSecond = BuildKeyMapping(second);
            if (keyedFirst.Count != keyedSecond.Count) { return false; }
            foreach (var entry in keyedSecond)
            {
                if (!keyedFirst.ContainsKey(entry.Key)) { return false; }
                if (!GetCachedSchema(entry.Key).Equals(keyedFirst[entry.Key], entry.Value)) { return false; }
            }
            return true;
        }

        public override string Format(JObject[] value) => $"[{string.Join(", ", value.Select(o => $"{{{KeySchema.Format(o[KeyProperty])}}}"))}]";

        private Schema<JObject> GetCachedSchema(string key) => _schemaCache.GetOrAdd(key, GetSchema);

        public override int GetHashCode(JObject[] value) => BuildKeyMapping(value).Select(entry => entry.Key.GetHashCode() ^ GetCachedSchema(entry.Key).GetHashCode(entry.Value)).Xor();

        protected override Schema<JObject> GetSchema(JToken token, int index) => GetSchema(token.AsObject(), index);

        protected override Schema<JObject> GetSchema(JObject value, int index)
        {
            // extract the key in small steps and get the schema based on it
            var property = value[KeyProperty] ?? DefaultKeyValue;
            if (property is null) throw new SchemaException($"Missing {KeyProperty} property.");
            var key = property.AsStringNoThrow();
            if (key is null) { throw new SchemaException($"Property {KeyProperty} is a {property.Type}, expected {JTokenType.String}."); }
            if (key.Length == 0) { throw new SchemaException($"Empty {KeyProperty} property."); }
            return GetCachedSchema(key);
        }

        protected abstract Schema<JObject> GetSchema(string key);

        public override JObject[] Merge(JObject[] first, JObject[] second)
        {
            // either simply combine the two or join by the key
            var keyedFirst = BuildKeyMapping(first);
            var keyedSecond = BuildKeyMapping(second);
            foreach (var entry in keyedSecond)
            {
                if (keyedFirst.ContainsKey(entry.Key))
                {
                    keyedFirst[entry.Key] = GetCachedSchema(entry.Key).Merge(keyedFirst[entry.Key], entry.Value);
                }
                else { keyedFirst.Add(entry); }
            }
            return keyedFirst.Values.ToArray();
        }

        protected override Control NewTemplate(Action<JObject> addItem)
        {
            Button addButton = new()
            {
                ID = "add",
                Text = "Add",
                CssClass = "uk-button uk-button-primary",
                UseSubmitBehavior = false,
            };
            var control = KeySchema.BuildControl
            (
                input: null,
                output: out var result,
                customEditButtons: addButton
            );
            addButton.Click += (s, e) =>
            {
                if (result(out var value))
                {
                    JObject obj = new(new JProperty(KeyProperty, value));
                    try { GetSchema(obj, -1); }
                    catch (SchemaException ex)
                    {
                        control.Page.Alert(ex.Message);
                        return;
                    }
                    addItem(obj);
                }
            };
            return control;
        }
    }

    public class SchemaObjectMap : SchemaObjectMapHelper, ISchemaInitializable<Schema>
    {
        public SchemaObjectMap(string title, string description, (string, SchemaString) key, JObject? defaultValue = null, bool editAllProperties = false) : base(title, description, key, key.Item2.DefaultValue) => Inner = new(title, description, defaultValue, editAllProperties);

        public SchemaObjectMap(string title, string description, (string, SchemaEnum) key, JObject? defaultValue = null, bool editAllProperties = false) : base(title, description, key, key.Item2.DefaultValueInternal) => Inner = new(title, description, defaultValue, editAllProperties);

        protected SchemaObject Inner { get; }

        public void Add(string name, Schema value) => Inner.Add(name, value);

        public IEnumerator<(string, Schema)> GetEnumerator() => Inner.GetEnumerator();

        protected override Schema<JObject> GetSchema(string key) => new SchemaObject(Inner, KeySchema.Format(key), string.Empty, defaultValue: new(new JProperty(KeyProperty, key)));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class SchemaString : SchemaValue<string>
    {
        private static readonly Regex _lookup = new(@"\$\{(?<name>[a-z]+)\}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public SchemaString(string title, string description, string defaultValue = "") : base(title, description) => DefaultValue = defaultValue;

        public override string DefaultValue { get; }

        public override Control BuildControl(string input, out Func<string> output)
        {
            TextBox textBox = new()
            {
                CssClass = "uk-input",
                Text = input,
            };
            output = () => textBox.Text;
            return textBox;
        }

        public override string Deserialize(JToken token) => token.AsString();

        public override string Expand(string value, Func<string, string?> lookup) => _lookup.Replace(value, match => lookup(match.Groups["name"].Value) ?? string.Empty);

        public override string Format(string value) => $"\"{value.Replace(@"\", @"\\")}\"";

        public override JToken Serialize(string value) => value;
    }

    public class SchemaStringArray : SchemaValueArray<SchemaString, string>
    {
        public SchemaStringArray(string title, string description, bool distinct = true) : base(new(title, description), distinct) { }

        protected override Control ItemTemplate(string item, int index, out SchemaResult<string> updateItem, Action removeItem)
        {
            TextBox textBox = new()
            {
                ID = "item",
                Text = item,
                CssClass = "uk-input uk-width-expand",
            };
            Button removeButton = new()
            {
                ID = "remove",
                Text = "Remove",
                CssClass = "uk-button uk-button-danger uk-margin-left uk-width-auto",
                UseSubmitBehavior = false,
            };
            removeButton.Click += (s, e) => removeItem();
            updateItem = (out string result) =>
            {
                result = textBox.Text ?? string.Empty;
                return true;
            };
            return NamingContainer.Wrap(
                Html.Div("uk-margin",
                    Html.Div("uk-flex",
                        textBox,
                        removeButton)));
        }

        protected override Control NewTemplate(Action<string> addItem)
        {
            TextBox textBox = new()
            {
                ID = "item",
                CssClass = "uk-input uk-width-expand",
            };
            Button addButton = new()
            {
                ID = "add",
                Text = "Add",
                CssClass = "uk-button uk-button-primary uk-margin-left uk-width-auto",
                UseSubmitBehavior = false,
            };
            addButton.Click += (s, e) => addItem(textBox.Text);
            Panel panel = new() { CssClass = "uk-margin uk-flex", DefaultButton = addButton.ID };
            panel.Controls.Add(textBox);
            panel.Controls.Add(addButton);
            return NamingContainer.Wrap(panel);
        }
    }

    public class SchemaLocalizedString : SchemaObject
    {
        private class SchemaMessage : SchemaString
        {
            public SchemaMessage(string title, string description) : base(title, description) { }

            public override JToken Serialize(string value)
            {
                if (value.Length > 4096) { throw new SchemaException("The maximum message length is 4096 characters."); }
                return base.Serialize(value);
            }
        }

        private const string DefaultMessageName = "defaultMessage";
        private const string LocalizedMessagesName = "localizedMessages";

        private static readonly SchemaMessage DefaultMessage = new("Default Message", "The default message displayed if no localized message is specified or the user's locale doesn't match with any of the localized messages. A default message must be provided if any localized messages are provided.");
        private static readonly SchemaObject LocalizedMessages = new("Localized Messages", "Translations of the message into different locales.");

        static SchemaLocalizedString()
        {
            foreach (var culture in Helpers.GetSetting("LANGUAGES").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(CultureInfo.GetCultureInfo).OrderBy(culture => culture.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                LocalizedMessages.Add(culture.Name, new SchemaMessage(culture.DisplayName, string.Empty));
            }
        }

        public SchemaLocalizedString(string title, string description) : base(title, description, editAllProperties: true)
        {
            base.Add(DefaultMessageName, DefaultMessage);
            base.Add(LocalizedMessagesName, LocalizedMessages);
        }

        public override void Add(string name, Schema value) => throw new NotSupportedException();

        public override JToken Serialize(JObject value)
        {
            if (value.ContainsKey(LocalizedMessagesName) && !value.ContainsKey(DefaultMessageName)) { throw new SchemaException("A default message must be provided if any localized messages are provided."); }
            return base.Serialize(value);
        }
    }

    public class SchemaBoolean : SchemaValue<bool>
    {
        public SchemaBoolean(string title, string description, bool defaultValue = false) : base(title, description) => DefaultValue = defaultValue;

        public override bool DefaultValue { get; }

        public override Control BuildControl(bool input, out Func<bool> output)
        {
            CheckBox checkBox = new() { Checked = input };
            checkBox.InputAttributes.Add("class", "uk-checkbox");
            output = () => checkBox.Checked;
            return checkBox;
        }

        public override bool Deserialize(JToken token) => token.AsBoolean();

        public override string Format(bool value) => value.ToString();

        public override JToken Serialize(bool value) => value;
    }

    public class SchemaInteger : SchemaValue<long>
    {
        public SchemaInteger(string title, string description, long defaultValue = 0) : base(title, description) => DefaultValue = defaultValue;

        public override long DefaultValue { get; }

        public int? Maximum { get; set; }

        public int? Minimum { get; set; } = 0;

        public override Control BuildControl(long input, out Func<long> output)
        {
            TextBox textBox = new()
            {
                CssClass = "uk-input",
                Text = input.ToString(CultureInfo.InvariantCulture),
            };
            textBox.Attributes["type"] = "number";
            textBox.Attributes["step"] = "1";
            if (Maximum.HasValue) { textBox.Attributes["max"] = Maximum.Value.ToString(CultureInfo.InvariantCulture); }
            if (Minimum.HasValue) { textBox.Attributes["min"] = Minimum.Value.ToString(CultureInfo.InvariantCulture); }
            output = () => string.IsNullOrEmpty(textBox.Text) ? 0 : long.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : throw new SchemaException($"'{textBox.Text}' is not a valid integer.");
            return textBox;
        }

        public override long Deserialize(JToken token) => token.AsInteger();

        public override string Format(long value) => value.ToString();

        public override JToken Serialize(long value)
        {
            if (value < 0) { throw new SchemaException("Value must be not be negative."); }
            if (value > Maximum) { throw new SchemaException($"Value must not exceed {Maximum}."); }
            return value;
        }
    }

    public class SchemaEnum : SchemaValue<int>, ISchemaInitializable<string>
    {
        private readonly string? _defaultValue;

        public SchemaEnum(string title, string description, string? defaultValue = null) : base(title, description) => _defaultValue = defaultValue;

        public override int DefaultValue => ResolveName(DefaultValueInternal);

        internal string DefaultValueInternal => _defaultValue ?? Values[0];

        protected SchemaEntries<string> Values { get; } = new();

        public void Add(string name, string value) => Values.Add(name, value);

        public override Control BuildControl(int input, out Func<int> output)
        {
            DropDownList dropDownList = new() { CssClass = "uk-select" };
            dropDownList.Items.AddRange(Enumerable.Range(0, Values.Count).Select<int, ListItem>(i => new()
            {
                Value = Values[i],
                Text = FormatLabel(i),
                Selected = i == input,
            }).ToArray());
            output = () => dropDownList.SelectedIndex;
            return dropDownList;
        }

        public override int Deserialize(JToken token) => ResolveName(token.AsString());

        public override string Format(int value) => Values[value];

        internal string FormatLabel(int index)
        {
            var name = Values[index];
            var description = Values[name];
            var separator = name.Length > 0 && description.Length > 0 ? ": " : string.Empty;
            return $"{name}{separator}{description}";
        }

        public IEnumerator<(string, string)> GetEnumerator() => Values.Entries.GetEnumerator();

        internal int ResolveName(string name)
        {
            var index = Values.IndexOf(name);
            return index > -1 ? index : throw new SchemaException($"'{name}' is not a valid value for {Title}.");
        }

        public override JToken Serialize(int value) => Values[value];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class SchemaFlags : SchemaValueArray<SchemaEnum, int>, ISchemaInitializable<string>
    {
        private readonly string[] _defaultValues;

        public SchemaFlags(string title, string description, params string[] defaultValues) : base(new(title, description), distinct: true) => _defaultValues = defaultValues.ToArray();

        public override int[] DefaultValue => _defaultValues.Select(Inner.ResolveName).Distinct().ToArray();

        public virtual void Add(string name, string value) => Inner.Add(name, value);

        public override Control BuildControl(int[] input, out Func<int[]> output)
        {
            var checkBoxes = Inner.Select((entry, index) =>
            {
                var (name, _) = entry;
                CheckBox checkBox = new()
                {
                    ID = name,
                    Text = $" {Inner.FormatLabel(index)}",
                    Checked = input.Contains(index),
                };
                checkBox.InputAttributes.Add("class", "uk-checkbox");
                return checkBox;
            }).ToArray();
            output = () => Enumerable.Range(0, checkBoxes.Length).Where(i => checkBoxes[i].Checked).ToArray();
            return NamingContainer.Wrap(Html.Div(checkBoxes.Select(checkBox => Html.Div(checkBox)).ToArray()));
        }

        public IEnumerator<(string, string)> GetEnumerator() => Inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class SchemaDuration : SchemaValue<TimeSpan>
    {
        public SchemaDuration(string title, string description) : base(title, description) { }

        public override TimeSpan DefaultValue => TimeSpan.Zero;

        public override Control BuildControl(TimeSpan input, out Func<TimeSpan> output)
        {
            TextBox textBox = new()
            {
                CssClass = "uk-input",
                Text = input.ToString(null, CultureInfo.InvariantCulture),
            };
            textBox.Attributes["type"] = "time";
            output = () => string.IsNullOrEmpty(textBox.Text)
                ? TimeSpan.Zero
                : TimeSpan.TryParse(textBox.Text, CultureInfo.InvariantCulture, out var result)
                    ? result
                    : throw new SchemaException($"Timespan '{textBox.Text}' is invalid.");
            return textBox;
        }

        public override TimeSpan Deserialize(JToken token)
        {
            var value = token.AsString();
            if (value.Length == 0 || value[value.Length - 1] != 's') { throw new SchemaException($"Duration '{value}' does not end in 's'."); }
            if (!double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result)) { throw new SchemaException($"Duration '{value}' is not a number."); }
            return TimeSpan.FromSeconds(result);
        }

        public override string Format(TimeSpan value) => value.ToString();

        public override JToken Serialize(TimeSpan value)
        {
            if (value < TimeSpan.Zero) { throw new SchemaException("Duration must not be negative."); }
            return Invariant($"{value.TotalSeconds:f9}s");
        }
    }
}
