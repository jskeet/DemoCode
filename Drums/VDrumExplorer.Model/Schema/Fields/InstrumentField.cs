// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Field representing an instrument within a kit. (More work required...)
    /// </summary>
    public sealed class InstrumentField : FieldBase
    {
        /// <summary>
        /// The offset within the same container for the single-byte instrument bank field,
        /// with a value of 0 for preset instruments and 1 for user samples.
        /// </summary>
        internal ModuleOffset BankOffset { get; }

        internal InstrumentField(FieldContainer? parent, FieldParameters common, ModuleOffset bankOffset) : base(parent, common) =>
            BankOffset = bankOffset;

        internal override FieldBase WithParent(FieldContainer parent) => new InstrumentField(parent, Parameters, BankOffset);
    }
}
