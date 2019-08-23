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
    public sealed class DataSegment
    {
        public static IComparer<DataSegment> AddressComparer { get; } = new AddressComparerImpl();

        private bool copyToSnapshotOnNextWrite = false;
        private byte[]? snapshot;
        
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

        public byte[] CopyData() => (byte[]) Data.Clone();

        public bool Contains(ModuleAddress other) =>
            other.CompareTo(Start) >= 0 && other.CompareTo(End) < 0;

        public byte this[ModuleAddress address]
        {
            get => Data[GetOffset(address)];
            set
            {
                var offset = GetOffset(address);
                if (Data[offset] != value)
                {
                    if (copyToSnapshotOnNextWrite)
                    {
                        snapshot = CopyData();
                        copyToSnapshotOnNextWrite = false;
                    }
                    Data[GetOffset(address)] = value;
                }
            }
        }

        internal void SetData(ModuleAddress address, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                // This handles overflow from 0x7f to 0x100 appropriately.
                this[address + i] = bytes[i];
            }
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

        /// <summary>
        /// Snapshots the data within this segment. Any previous snapshot is lost.
        /// </summary>
        public void Snapshot()
        {
            copyToSnapshotOnNextWrite = true;
            snapshot = null;
        }

        /// <summary>
        /// Commits the snapshot, forgetting previous data.
        /// </summary>
        /// <returns>true if changes had been made since the snapshot was taken, false otherwise.</returns>
        public bool CommitSnapshot()
        {
            bool ret = snapshot != null;
            snapshot = null;
            copyToSnapshotOnNextWrite = false;
            return ret;
        }

        /// <summary>
        /// Reverts the snapshot, forgetting changes made since the snapshot was taken.
        /// </summary>
        /// <returns>true if changes had been made since the snapshot was taken, false otherwise.</returns>
        public bool RevertSnapshot()
        {
            copyToSnapshotOnNextWrite = false;
            if (snapshot != null)
            {
                Buffer.BlockCopy(snapshot, 0, Data, 0, snapshot.Length);
                snapshot = null;
                return true;
            }
            return false;
        }

        private class AddressComparerImpl : IComparer<DataSegment>
        {
            public int Compare(DataSegment x, DataSegment y) => x.Start.CompareTo(y.Start);
        }
    }
}
