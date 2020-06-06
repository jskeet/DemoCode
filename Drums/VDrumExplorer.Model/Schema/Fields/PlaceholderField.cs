// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field which is effectively just a placeholder to allow offsets to compute nicely.
    /// This is never exposed; field containers skip placeholder fields during construction.
    /// </summary>
    internal class PlaceholderField : FieldBase
    {
        internal PlaceholderField(FieldContainer? parent, FieldParameters common) : base(parent, common)
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) => new PlaceholderField(parent, Parameters);
    }
}
