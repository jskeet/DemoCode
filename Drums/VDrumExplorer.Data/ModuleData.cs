// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// The data within a module, as a collection of populated non-overlapping segments.
    /// </summary>
    public class ModuleData
    {
        private readonly object sync = new object();
        private readonly List<Segment> segments = new List<Segment>();

        /// <summary>
        /// Creates an empty instance which can then be populated.
        /// </summary>
        public ModuleData()
        {
        }

        public void Populate(ModuleAddress address, byte[] data)
        {
            if (data.Length >= 0x100)
            {
                throw new ArgumentException("Data size must be less than 0x100");
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

        public static ModuleData Load(Stream stream) => Load(new BinaryReader(stream, Encoding.UTF8, true));

        public static ModuleData Load(BinaryReader reader)
        {
            var data = new ModuleData();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var segment = Segment.Load(reader);
                // TODO: Use the public API to add the segments, so that ordering doesn't matter?
                data.segments.Add(segment);
            }
            return data;
        }

        public void Save(Stream stream) => Save(new BinaryWriter(stream, Encoding.UTF8, true));

        public void Save(BinaryWriter writer)
        {
            List<Segment> localSegments;
            lock (sync)
            {
                localSegments = segments.ToList();
            }

            writer.Write(localSegments.Count);
            foreach (var segment in localSegments)
            {
                segment.Save(writer);
            }
        }

        public bool HasData(ModuleAddress address) => GetSegmentOrNull(address) != null;

        private Segment GetSegment(ModuleAddress address) =>
            GetSegmentOrNull(address) ?? throw new ArgumentException($"No data found for {address}");

        private Segment? GetSegmentOrNull(ModuleAddress address)
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

        /// <summary>
        /// Writes data into an existing (single) segment.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="bytes">The bytes to write.</param>
        internal void SetData(ModuleAddress address, byte[] bytes)
        {
            var segment = GetSegment(address);
            for (int i = 0; i < bytes.Length; i++)
            {
                // This handles overflow from 0x7f to 0x100 appropriately.
                segment[address + i] = bytes[i];
            }
        }

        internal void SetAddressValue(ModuleAddress address, byte value) =>
            GetSegment(address)[address] = value;

        private class Segment
        {
            public ModuleAddress Start { get; }
            private byte[] Data { get; }
            public ModuleAddress End { get; }

            public Segment(ModuleAddress start, byte[] data) =>
                (Start, Data, End) = (start, data, start + data.Length);

            public bool Contains(ModuleAddress other) =>
                other.CompareTo(Start) >= 0 && other.CompareTo(End) < 0;

            public byte this[ModuleAddress address]
            {
                get => Data[GetOffset(address)];
                set => Data[GetOffset(address)] = value;
            }

            private int GetOffset(ModuleAddress address)
            {
                int offset = address - Start;
                if (offset >= 0x100)
                {
                    offset -= 0x80;
                }
                return offset;
            }

            public void Save(BinaryWriter writer)
            {
                writer.Write(Start.Value);
                writer.Write(Data.Length);
                writer.Write(Data);
            }

            public static Segment Load(BinaryReader reader)
            {
                var address = new ModuleAddress(reader.ReadInt32());
                var length = reader.ReadInt32();
                var data = reader.ReadBytes(length);
                return new Segment(address, data);
            }
        }

        private class SegmentAddressComparer : IComparer<Segment>
        {
            public static SegmentAddressComparer Instance = new SegmentAddressComparer();

            public int Compare(Segment x, Segment y) => x.Start.CompareTo(y.Start);
        }
    }
}
