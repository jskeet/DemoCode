// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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
        /// Loads module data, autodetecting the schema.
        /// </summary>
        public static ModuleData FromStream(Stream stream)
        {
            Header header;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                header = Header.Load(reader);
                foreach (var schema in SchemaRegistry.GetSchemas())
                {
                    if (header.Equals(Header.FromSchema(schema)))
                    {
                        var ret = new ModuleData(schema);
                        ret.LoadData(reader);
                        return ret;
                    }
                }
            }
            throw new InvalidOperationException($"No built-in schemas match the file's header ({header})");
        }

        private void LoadData(BinaryReader reader)
        {
            List<Segment> localSegments = new List<Segment>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                localSegments.Add(Segment.Load(reader));
            }
            lock (sync)
            {
                segments.Clear();
                segments.AddRange(localSegments);
            }
        }

        /// <summary>
        /// Loads memory data from the given stream. Any existing data is cleared, if this operation is successful.
        /// </summary>
        public void Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var streamHeader = Header.Load(reader);
                var schemaHeader = Header.FromSchema(Schema);
                if (!streamHeader.Equals(schemaHeader))
                {
                    throw new InvalidOperationException($"Stream data does not match schema. Stream header: {streamHeader}. Schema header: {schemaHeader}");
                }
                LoadData(reader);
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
                var header = Header.FromSchema(Schema);
                header.Save(writer);
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

        private sealed class Header : IEquatable<Header?>
        {
            public const int CurrentFormatVersion = 1;
            
            public int FormatVersion { get; }
            public int ModelId { get; }
            public int FamilyCode { get; }
            public int FamilyNumberCode { get; }
            public string Name { get; }

            public Header(int formatVersion, int modelId, int familyCode, int familyNumberCode, string name) =>
                (FormatVersion, ModelId, FamilyCode, FamilyNumberCode, Name) = (formatVersion, modelId, familyCode, familyNumberCode, name);

            public void Save(BinaryWriter writer)
            {
                writer.Write(FormatVersion);
                writer.Write(ModelId);
                writer.Write(FamilyCode);
                writer.Write(FamilyNumberCode);
                writer.Write(Name);
            }

            public static Header Load(BinaryReader reader)
            {
                var version = reader.ReadInt32();
                if (version != CurrentFormatVersion)
                {
                    throw new InvalidOperationException($"Unknown file format version. Expected {CurrentFormatVersion}; was {version}");
                }
                var modelId = reader.ReadInt32();
                var familyCode = reader.ReadInt32();
                var familyNumberCode = reader.ReadInt32();
                var name = reader.ReadString();
                return new Header(version, modelId, familyCode, familyNumberCode, name);
            }

            public override bool Equals(object? obj) => Equals(obj as Header);
            
            public bool Equals(Header? other) =>
                other != null &&
                FormatVersion == other.FormatVersion &&
                Name == other.Name &&
                ModelId == other.ModelId &&
                FamilyCode == other.FamilyCode &&
                FamilyNumberCode == other.FamilyNumberCode;

            public override int GetHashCode() => HashCode.Combine(FormatVersion, ModelId, FamilyCode, FamilyNumberCode, Name.GetHashCode());

            // TODO: Work out how to handle compatibility.
            public static Header FromSchema(ModuleSchema schema) =>
                new Header(CurrentFormatVersion, schema.ModelId, schema.FamilyCode, schema.FamilyNumberCode, schema.Name);

            public override string ToString() => $"Name: {Name}; Midi ID: {ModelId}; Family code: {FamilyCode}; Family number code: {FamilyNumberCode}";
        }
    }
}
