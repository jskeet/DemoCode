// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// Field representing an instrument within a kit. (More work required...)
    /// </summary>
    public sealed class InstrumentField : FieldBase
    {
        /// <summary>
        /// The field within the parent container that determines whether the instrument is
        /// a preset instrument or a user sample.
        /// </summary>
        private readonly string bankField;

        internal InstrumentField(Parameters common, string bankField) : base(common) =>
            this.bankField = bankField;

        internal override bool ValidateData(FieldContainerData data, out string? error)
        {
            // FIXME: Implement.
            error = null;
            return true;
        }
    }
}
