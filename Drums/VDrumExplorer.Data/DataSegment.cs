// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// A segment of data within a <see cref="ModuleData"/>.
    /// </summary>
    internal sealed class DataSegment
    {
        public static IComparer<DataSegment> AddressComparer { get; } = new AddressComparerImpl();
            
        public ModuleAddress Start { get; }
        private byte[] Data { get; }
        public ModuleAddress End { get; }

        public DataSegment(ModuleAddress start, byte[] data)
        {
            if (data.Length > 0xff)
            {
                throw new ArgumentOutOfRangeException("Data segments cannot (currently) be more than 255 bytes long");
            }
            (Start, Data, End) = (start, data, start + data.Length);
        }

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
            if (offset >= 0x80)
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

        public static DataSegment Load(BinaryReader reader)
        {
            var address = new ModuleAddress(reader.ReadInt32());
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);
            return new DataSegment(address, data);
        }

        private class AddressComparerImpl : IComparer<DataSegment>
        {
            public int Compare(DataSegment x, DataSegment y) => x.Start.CompareTo(y.Start);
        }
    }
}
