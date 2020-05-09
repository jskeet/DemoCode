// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// A snapshot of module data. This is the unit of loading and saving; see
    /// <see cref="ModuleData.LoadSnapshot(ModuleDataSnapshot)"/> and
    /// <see cref="ModuleData.CreateSnapshot"/>.
    /// </summary>
    public sealed class ModuleDataSnapshot
    {
        private readonly SortedDictionary<ModuleAddress, DataSegment> segments;

        public IEnumerable<DataSegment> Segments => segments.Values;

        public ModuleDataSnapshot()        
        {
            segments = new SortedDictionary<ModuleAddress, DataSegment>();
        }

        public int SegmentCount => segments.Count;

        internal DataSegment this[ModuleAddress address] => segments[address];

        public bool TryGetSegment(ModuleAddress address, out DataSegment segment) =>
            segments.TryGetValue(address, out segment);

        public void Add(DataSegment segment) => segments.Add(segment.Address, segment);
    }
}
