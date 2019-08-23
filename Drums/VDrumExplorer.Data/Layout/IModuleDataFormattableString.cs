// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Layout
{
    /// <summary>
    /// An element of a module that can be formatted, when provided with module data (which may
    /// or may not be used).
    /// </summary>
    public interface IModuleDataFormattableString
    {
        /// <summary>
        /// Formats the value with respect to the given module data.
        /// </summary>
        string Format(ModuleData data);
        
        /// <summary>
        /// The address of data that is being formatted, or null if the value is fixed.
        /// </summary>
        ModuleAddress? Address { get; }
    }
}
