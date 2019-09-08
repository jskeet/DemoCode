// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// Common interface for all simple fields (numbers, enums etc).
    /// </summary>
    public interface IPrimitiveField : IField
    {
        /// <summary>
        /// Formats the value of this field from within the specified module data.
        /// </summary>
        /// <param name="context">The context in which to resolve the field.</param>
        /// <param name="data">The data to fetch the value from.</param>
        /// <returns>The formatted value.</returns>
        string GetText(FixedContainer context, ModuleData data);

        // TODO: Move this down to NumericField/StringField?
        /// <summary>
        /// Attempts to set the given value within the module data after parsing from text.
        /// </summary>
        /// <param name="context">The context in which to resolve the field.</param>
        /// <param name="data">The data in which to set the parsed value.</param>
        /// <param name="text">The text to parse.</param>
        /// <returns>true if the value is valid; false otherwise.</returns>
        bool TrySetText(FixedContainer context, ModuleData data, string text);

        /// <summary>
        /// Sets the field to a valid default value within the module data.
        /// </summary>
        /// <param name="context">The context in which to resolve the field.</param>
        /// <param name="data">The data in which to reset the field value.</param>
        void Reset(FixedContainer context, ModuleData data);

        /// <summary>
        /// Validates the field against specific module data.
        /// </summary>
        /// <param name="context">The context in which to resolve the field.</param>
        /// <param name="data">The data to validate against the field.</param>
        /// <param name="error">The error message, if any. Null otherwise.</param>
        /// <returns>true if the data is valid for this field; false otherwise.</returns>
        bool Validate(FixedContainer context, ModuleData data, out string? error);
    }
}
