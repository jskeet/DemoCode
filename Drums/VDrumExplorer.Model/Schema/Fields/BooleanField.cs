// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field representing a Boolean value.
    /// </summary>
    public sealed class BooleanField : NumericFieldBase
    {
        internal BooleanField(FieldContainer? parent, FieldParameters common)
            : base(parent, common, min: 0, max: 1, @default: 0)
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new BooleanField(parent, Parameters);
    }
}
