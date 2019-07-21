using System;
using System.Collections.Generic;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models
{
    public class ModuleData
    {
        public ModuleFields ModuleFields { get; }

        private readonly object sync = new object();
        private readonly List<Segment> segments = new List<Segment>();

        public ModuleData(ModuleFields moduleFields) =>
            ModuleFields = moduleFields;

        public void Populate(ModuleAddress address, byte[] data)
        {
            if (data.Length >= 0x100)
            {
                throw new ArgumentException("Data size must be less than 0x100");
            }
            // Expand the data if necessary, to fit the compressed address space more simply.
            if (data.Length > 0x7f)
            {
                byte[] newData = new byte[data.Length + 0x80];
                Buffer.BlockCopy(data, 0, newData, 0, 0x7f);
                Buffer.BlockCopy(data, 0x80, newData, 0x100, data.Length - 0x80);
                data = newData;
            }
            var segment = new Segment(address, data);

            lock (sync)
            {
                int index = segments.BinarySearch(segment, SegmentAddressComparer.Instance);
                if (index >= 0)
                {
                    throw new ArgumentException("Segment already exists");
                }
                segments.Insert(~index, segment);
            }
        }

        public bool HasData(ModuleAddress address) => GetSegmentOrNull(address) != null;

        private Segment GetSegment(ModuleAddress address) =>
            GetSegmentOrNull(address) ?? throw new ArgumentException($"No data found for {address}");

        private Segment GetSegmentOrNull(ModuleAddress address)
        {
            lock (sync)
            {
                // Binary search on start addresses, expecting not to find an exact match necessarily.
                // TODO: This shouldn't be necessary, surely
                int lowInc = 0;
                int highExc = segments.Count;
                while (lowInc < highExc)
                {
                    int candidate = (lowInc + highExc) / 2;
                    ModuleAddress candidateAddress = segments[candidate].Start;
                    var comparison = candidateAddress.CompareTo(address);
                    // Exact match! Great, can exit immediately.
                    if (comparison == 0)
                    {
                        return segments[candidate];
                    }
                    else if (comparison < 0)
                    {
                        lowInc = candidate + 1;
                    }
                    else
                    {
                        highExc = candidate;
                    }
                }
                // No exact match, but it's possible (likely!) that we found a match in "lowInc-1", with
                // a start address greater than the target, but which contains the target.
                if (lowInc > 0)
                {
                    var segment = segments[lowInc - 1];
                    if (segment.Contains(address))
                    {
                        return segment;
                    }
                }
                return null;
            }
        }

        public byte GetAddressValue(ModuleAddress address) => GetSegment(address)[address];

        public byte[] GetData(ModuleAddress address, int size)
        {
            var segment = GetSegment(address);
            byte[] ret = new byte[size];
            for (int i = 0; i < size; i++)
            {
                // This handles overflow from 0x7f to 0x100 appropriately.
                ret[i] = segment[address + i];
            }
            return ret;
        }

        private class Segment
        {
            public ModuleAddress Start { get; }
            public byte[] Data { get; }
            public ModuleAddress End { get; }

            public Segment(ModuleAddress start, byte[] data) =>
                (Start, Data, End) = (start, data, start + data.Length);

            public bool Contains(ModuleAddress other) =>
                other.CompareTo(Start) >= 0 && other.CompareTo(End) < 0;

            public byte this[ModuleAddress address] => Data[address - Start];
        }

        private class SegmentAddressComparer : IComparer<Segment>
        {
            public static SegmentAddressComparer Instance = new SegmentAddressComparer();

            public int Compare(Segment x, Segment y) => x.Start.CompareTo(y.Start);
        }
    }
}
