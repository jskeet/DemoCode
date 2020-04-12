// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field representing a Boolean value.
    /// </summary>
    public sealed class BooleanField : NumericFieldBase
    {
        internal BooleanField(Parameters common)
            : base(common, min: 0, max: 0, @default: 0)
        {
        }
    }
}
