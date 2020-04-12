// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// Information about a data change within a <see cref="FieldContainerData"/>.
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The container containing the changed data.
        /// </summary>
        public FieldContainerData Container { get; }

        /// <summary>
        /// The first offset within the container data that changed.
        /// </summary>
        public ModuleOffset Start { get; }

        /// <summary>
        /// The offset after the final changed data (so an exclusive upper bound).
        /// </summary>
        public ModuleOffset End { get; }

        public DataChangedEventArgs(FieldContainerData container, ModuleOffset start, ModuleOffset end) =>
            (Container, Start, End) = (container, start, end);

        // TODO: Test this!
        public bool OverlapsField(FieldContainer targetContainer, IField targetField)
        {
            if (targetContainer != Container.FieldContainer)
            {
                return false;
            }

            int targetStart = targetField.Offset.LogicalValue;
            int targetEnd = targetStart + targetField.Size;

            int sourceStart = Start.LogicalValue;
            int sourceEnd = End.LogicalValue;

            // The two ranges overlap if the "latest start" is earlier than the "earliest end".
            return Math.Max(targetStart, sourceStart) < Math.Min(targetEnd, sourceEnd);
        }
    }
}
