// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// A field which is effectively just a placeholder to allow offsets to compute nicely.
    /// This is never exposed; field containers skip placeholder fields during construction.
    /// </summary>
    internal class PlaceholderField : FieldBase
    {
        internal PlaceholderField(Parameters common) : base(common)
        {
        }

        internal override bool ValidateData(FieldContainerData data, out string? error) =>
            throw new NotSupportedException("Place holder fields should never get as far as validation.");
    }
}
