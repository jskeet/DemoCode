// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data
{
    public sealed class DataSegment
    {
        public ModuleAddress Address { get; }
        public int Size => data.Length;
        private readonly byte[] data;

        public DataSegment(ModuleAddress address, byte[] data) =>
            (Address, this.data) = (address, data);

        internal DataSegment Clone() => new DataSegment(Address, CopyData());

        /// <summary>
        /// Creates a new data segment sharing the data with this one, but at the given new address.
        /// </summary>
        internal DataSegment WithAddress(ModuleAddress newAddress) =>
            new DataSegment(newAddress, data);

        /// <summary>
        /// Creates a copy of the data in this container.
        /// </summary>
        /// <returns>A copy of the data.</returns>
        public byte[] CopyData() => (byte[]) data.Clone();

        /// <summary>
        /// Reads a 32-bit integer from the data, starting at the given offset.
        /// The expected format of the data depends on how many bytes are being read.
        /// A single-byte value uses all 7 usable bits; two-byte and four-byte values each
        /// use 4 bits from each byte.
        /// 3-byte values (only used on the Aerophone) use all seven bits.
        /// </summary>
        /// <param name="offset">The offset for the start of the data.</param>
        /// <param name="size">The codec to use to interpret the data.</param>
        /// <returns>An integer read from the data.</returns>
        internal int ReadInt32(ModuleOffset offset, NumericCodec codec)
        {
            ValidateRange(offset, codec.Size);
            var slice = data.AsSpan().Slice(offset.LogicalValue, codec.Size);
            return codec.ReadInt32(slice);
        }

        internal void WriteInt32(ModuleOffset offset, NumericCodec codec, int value)
        {
            ValidateRange(offset, codec.Size);
            var slice = data.AsSpan().Slice(offset.LogicalValue, codec.Size);
            codec.WriteInt32(slice, value);
        }

        /// <summary>
        /// Copies part of the data into the specified span.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="destination">The span to copy data into.</param>
        internal void ReadBytes(ModuleOffset offset, Span<byte> destination)
        {
            ValidateRange(offset, destination.Length);
            data.AsSpan().Slice(offset.LogicalValue, destination.Length).CopyTo(destination);
        }

        internal void WriteBytes(ModuleOffset offset, ReadOnlySpan<byte> bytes)
        {
            ValidateRange(offset, bytes.Length);
            var targetSpan = data.AsSpan().Slice(offset.LogicalValue);
            bytes.CopyTo(targetSpan);
        }

        private void ValidateRange(ModuleOffset offset, int length)
        {
            int start = offset.LogicalValue;
            if (start < 0 || start >= data.Length)
            {
                throw new ArgumentException($"Invalid index ({offset.LogicalValue}) into container of length {data.Length}");
            }

            int exclusiveEnd = start + length;
            if (exclusiveEnd > data.Length)
            {
                throw new ArgumentException($"Invalid length ({length}) from index ({offset.LogicalValue}) into container of length {data.Length}");
            }
        }
    }
}
