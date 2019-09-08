// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    /// <summary>
    /// An element of a module that can be formatted, when provided with module data (which may
    /// or may not be used).
    /// </summary>
    internal interface IModuleDataFormattableString
    {
        /// <summary>
        /// Formats the value with respect to the given module data.
        /// </summary>
        string Format(FixedContainer context, ModuleData data);

        ModuleAddress? GetSegmentAddress(FixedContainer context);
    }
}
