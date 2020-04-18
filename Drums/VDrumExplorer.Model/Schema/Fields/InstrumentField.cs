// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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

        internal InstrumentField(Parameters common, ModuleOffset bankOffset) : base(common) =>
            BankOffset = bankOffset;
    }
}
