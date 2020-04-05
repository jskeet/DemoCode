// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// A dynamic overlay within a <see cref="FieldContainer"/>, as if it were a single field.
    /// This is (primarily) used for VEdit and MultiFX where the meaning of
    /// the parameter fields depends on which instrument or FX type is selected.
    /// </summary>
    public sealed class OverlayField : FieldBase
    {
        /// <summary>
        /// The absolute path of the field that determines which overlay is applied.
        /// </summary>
        public string SwitchPath { get; }

        /// <summary>
        /// A list to be indexed by the value of the switch field (raw numeric value, or instrument
        /// group number for instrument fields) to access field list in the overlay.
        /// </summary>
        public IReadOnlyList<FieldList> FieldLists { get; }

        /// <summary>
        /// The maximum number of fields within each field list.
        /// </summary>
        public int NestedFieldCount { get; }

        internal OverlayField(Parameters common, int nestedFieldCount, string switchPath, IReadOnlyList<FieldList> fieldLists) : base(common) =>
            (NestedFieldCount, SwitchPath, FieldLists) = (nestedFieldCount, switchPath, fieldLists);

        internal OverlayField WithPath(string newSwitchPath) =>
            newSwitchPath == SwitchPath ? this : new OverlayField(new Parameters(Name, Description, Offset, Size), NestedFieldCount, newSwitchPath, FieldLists);

        internal override bool ValidateData(FieldContainerData data, out string? error)
        {
            // FIXME: Implement.
            error = null;
            return true;
        }

        public sealed class FieldList
        {
            public string Description { get; }
            public IReadOnlyList<IField> Fields { get; }

            public FieldList(string description, IReadOnlyList<IField> fields) =>
                (Description, Fields) = (description, fields);

            public override string ToString() => Description;
        }
    }
}
