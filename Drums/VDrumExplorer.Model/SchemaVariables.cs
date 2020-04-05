// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// Used within schema population to keep track of things like the kit number.
    /// </summary>
    internal class SchemaVariables
    {
        private readonly SchemaVariables? parent;
        private readonly string key;
        private readonly string value;
        private readonly string template;

        internal static SchemaVariables Empty { get; } =
            new SchemaVariables(null, "", "");

        private SchemaVariables(SchemaVariables? parent, string key, string value) =>
            (this.parent, this.key, this.value, template) = (parent, key, value, '{' + key + '}');

        internal SchemaVariables WithVariable(string? key, string value) =>
            key is null ? this : new SchemaVariables(this, key, value);

        internal SchemaVariables WithVariables(IDictionary<string, string>? variables)
        {
            if (variables is null)
            {
                return this;
            }
            var current = this;
            foreach (var pair in variables)
            {
                current = current.WithVariable(pair.Key, pair.Value);
            }
            return current;
        }

        private string? this[string key] => this.key == key ? value : parent?[key];

        internal string Replace(string text)
        {
            // If we're at the root variable, or there are no braces in the string,
            // we're done.
            if (parent is null || text.IndexOf('{') == -1)
            {
                return text;
            }
            // Perform our replacement.
            text = text.Replace(template, value);
            // Ask our parent to perform its replacement.
            return parent.Replace(text);
        }
    }
}
