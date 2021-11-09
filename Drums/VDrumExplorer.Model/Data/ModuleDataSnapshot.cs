// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// A snapshot of module data. This is the unit of loading and saving; see
    /// <see cref="ModuleData.LoadSnapshot(ModuleDataSnapshot)"/> and
    /// <see cref="ModuleData.CreateSnapshot"/>.
    /// </summary>
    public sealed class ModuleDataSnapshot
    {
        private readonly SortedDictionary<ModuleAddress, DataSegment> segmentsByAddress;

        public IEnumerable<DataSegment> Segments => segmentsByAddress.Values;

        public ModuleDataSnapshot()        
        {
            segmentsByAddress = new SortedDictionary<ModuleAddress, DataSegment>();
        }

        public int SegmentCount => segmentsByAddress.Count;

        internal DataSegment this[ModuleAddress address] => segmentsByAddress[address];

        public bool TryGetSegment(ModuleAddress address, out DataSegment segment) =>
            segmentsByAddress.TryGetValue(address, out segment);

        public void Add(DataSegment segment) => segmentsByAddress.Add(segment.Address, segment);

        /// <summary>
        /// Creates a new snapshot with all the segments in this one, but relocated
        /// such that the data is <paramref name="from"/> is moved to <paramref name="to"/>
        /// (and all other data is offset by the same amount).
        /// </summary>
        public ModuleDataSnapshot Relocated(ModuleAddress from, ModuleAddress to)
        {
            var offset = to.LogicalValue - from.LogicalValue;
            var snapshot = new ModuleDataSnapshot();
            foreach (var segment in Segments)
            {
                snapshot.Add(segment.WithAddress(segment.Address.PlusLogicalOffset(offset)));
            }
            return snapshot;
        }

        /// <summary>
        /// Convenience method to call <see cref="Relocated(ModuleAddress, ModuleAddress)"/>
        /// with addresses taken from logical tree nodes.
        /// </summary>
        public ModuleDataSnapshot Relocated(TreeNode from, TreeNode to)
        {
            var fromAddress = from.DescendantFieldContainers().Min(fc => fc.Address);
            var toAddress = to.DescendantFieldContainers().Min(fc => fc.Address);
            return Relocated(fromAddress, toAddress);
        }
    }
}
