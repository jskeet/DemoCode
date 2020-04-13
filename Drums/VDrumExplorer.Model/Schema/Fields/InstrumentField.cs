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
        /// The path to the field (usually within the same container) that determines whether the instrument is
        /// a preset instrument or a user sample.
        /// </summary>
        internal string BankPath { get; }

        internal InstrumentField(Parameters common, string bankPath) : base(common) =>
            BankPath = bankPath;
    }
}
