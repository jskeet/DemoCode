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
        /// <param name="data">The data to fetch the value from.</param>
        /// <returns>The formatted value.</returns>
        string GetText(ModuleData data);

        /// <summary>
        /// Attempts to set the given value within the module data after parsing from text.
        /// </summary>
        /// <param name="data">The data in which to set the parsed value.</param>
        /// <param name="text">The text to parse.</param>
        /// <returns>true if the value is valid; false otherwise.</returns>
        bool TrySetText(ModuleData data, string text);
    }
}
