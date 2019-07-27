using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data
{
    public class ModuleData
    {
        public ModuleSchema Schema { get; }

        private readonly object sync = new object();
        private readonly List<Segment> segments = new List<Segment>();

        public ModuleData(ModuleSchema schema) =>
            Schema = schema;

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

        /// <summary>
        /// Loads memory data from the given stream. Any existing data is cleared, if this operation is successful.
        /// </summary>
        public void Load(Stream stream)
        {
            List<Segment> localSegments = new List<Segment>();
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var name = reader.ReadString();
                var midiId = reader.ReadInt32();
                if (name != Schema.Name)
                {
                    throw new InvalidOperationException($"Expected data for module name '{Schema.Name}'; received '{name}'");
                }
                if (midiId != Schema.MidiId)
                {
                    throw new InvalidOperationException($"Expected data for module with Midi ID '{Schema.MidiId}; received '{midiId}'");
                }
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    localSegments.Add(Segment.Load(reader));
                }
            }
            lock (sync)
            {
                segments.Clear();
                segments.AddRange(localSegments);
            }
        }

        public void Save(Stream stream)
        {
            List<Segment> localSegments;
            lock (sync)
            {
                localSegments = segments.ToList();
            }
            
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Schema.Name);
                writer.Write(Schema.MidiId);
                writer.Write(localSegments.Count);
                foreach (var segment in localSegments)
                {
                    segment.Save(writer);
                }                
            }
        }

        /// <summary>
        /// Validates that every field in the schema has a valid value.
        /// This will fail if only partial data has been loaded.
        /// </summary>
        public void Validate()
        {
            List<Exception> exceptions = new List<Exception>();
            foreach (var field in Schema.Root.DescendantsAndSelf(this).OfType<IPrimitiveField>())
            {
                try
                {
                    field.GetText(this);
                }
                catch (Exception e)
                {
                    exceptions.Add(new InvalidOperationException($"Field {field.Path} failed validation: {e.Message}"));
                }
            }
            if (exceptions.Count != 0)
            {
                throw new AggregateException("Validation failed", exceptions.ToArray());
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
                get
                {
                    int offset = address - Start;
                    if (offset >= 0x100)
                    {
                        offset -= 0x80;
                    }
                    return Data[offset];
                }
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
