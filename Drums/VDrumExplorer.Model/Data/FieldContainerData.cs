﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.PhysicalSchema;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// Data for an individual <see cref="FieldContainer"/>.
    /// </summary>
    public sealed class FieldContainerData
    {
        public ModuleAddress Address { get; }
        private readonly byte[] data;

        public FieldContainerData(ModuleAddress address, byte[] data) =>
            (Address, this.data) = (address, Preconditions.CheckNotNull(data, nameof(data)));

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
        /// </summary>
        /// <param name="offset">The offset for the start of the data.</param>
        /// <param name="size">The number of bytes to read. Must be 1, 2 or 4.</param>
        /// <returns>An integer read from the data.</returns>
        public int ReadInt32(ModuleOffset offset, int size)
        {
            ValidateRange(offset, size);
            int start = offset.LogicalValue;
            return size switch
            {
                1 => data[start],
                2 => (sbyte) ((data[start] << 4) | data[start + 1]),
                4 => (short) (
                    (data[start] << 12) |
                    (data[start + 1] << 8) |
                    (data[start + 2] << 4) |
                    (data[start + 3] << 0)),
                _ => throw new InvalidOperationException($"Cannot read numeric value with size {size}")
            };
        }

        // TODO: Span<byte> perhaps?

        /// <summary>
        /// Copies part of the data into the specified span.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="destination">The span to copy data into.</param>
        public void ReadBytes(ModuleOffset offset, Span<byte> destination)
        {
            ValidateRange(offset, destination.Length);
            data.AsSpan().Slice(offset.LogicalValue, destination.Length).CopyTo(destination);
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
