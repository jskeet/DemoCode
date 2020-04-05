// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace VDrumExplorer.Model.Json
{
    /// <summary>
    /// Base class for <see cref="FieldJson"/> and <see cref="ContainerReferenceJson"/> with common properties.
    /// </summary>
    internal abstract class ContainerItemBase
    {
        // Note on Name and Description:
        // - At least one of them must be set.
        // - If just Name is set, Description defaults to Name
        // - If just Description is set, Name defaults to Description with punctuation removed but used as word separators, and in PascalCase
        //   (so a description of "Foo bar" would lead to a name of "FooBar")

        /// <summary>
        /// Name of the item in the path.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Description to display. For repeated fields, this is a format string,
        /// where {0} will be replaced with the element index, and {1} will be replaced
        /// with the value from <see cref="RepeatJson.DescriptionLookup"/>, if any.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The details of how this item is repeated, if any.
        /// </summary>
        public RepeatJson? Repeat { get; set; }

        // The resolved name and description (based on Name and Description properties) populated during validation.
        protected string? ResolvedName { get; set; }
        protected string? ResolvedDescription { get; set; }

        internal virtual void ValidateJson(ModuleJson module)
        {
            ResolvedName = Validation.ValidateNotNull(Name ?? ToPascalCase(Description), "Neither Name nor Description is set");
            ResolvedDescription = Description ?? Name;
        }

        [return:NotNullIfNotNull("text")]
        private static string? ToPascalCase(string? text)
        {
            if (text is null)
            {
                return null;
            }
            var builder = new StringBuilder();
            bool upperCaseNext = false;
            foreach (char c in text)
            {
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                        builder.Append(upperCaseNext ? char.ToUpper(c, CultureInfo.InvariantCulture) : c);
                        upperCaseNext = false;
                        break;
                    case UnicodeCategory.UppercaseLetter:
                        builder.Append(c);
                        upperCaseNext = false;
                        break;
                    case UnicodeCategory.DecimalDigitNumber:
                        builder.Append(c);
                        upperCaseNext = true;
                        break;
                    default:
                        upperCaseNext = true;
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
