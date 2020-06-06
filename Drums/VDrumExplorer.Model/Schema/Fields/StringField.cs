// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field representing textual data, such as the name of a kit.
    /// </summary>
    public sealed class StringField : FieldBase
    {
        /// <summary>
        /// The length of the field, in characters.
        /// </summary>
        public int Length { get; }

        internal int BytesPerChar { get; }

        internal StringField(FieldContainer? parent, FieldParameters common, int length)
            : base(parent, common)
        {
            Length = length;
            BytesPerChar = Size / length;
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new StringField(parent, Parameters, Length);
    }
}
