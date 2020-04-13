// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// Data for an individual <see cref="FieldContainer"/>.
    /// </summary>
    public sealed class FieldContainerData
    {
        public ModuleData ModuleData { get; }
        public FieldContainer FieldContainer { get; }
        private readonly byte[] data;

        public event EventHandler<DataChangedEventArgs>? DataChanged;

        public FieldContainerData(ModuleData moduleData, FieldContainer fieldContainer, byte[] data) =>
            (ModuleData, FieldContainer, this.data) =
            (moduleData, fieldContainer, Preconditions.CheckNotNull(data, nameof(data)));

        /// <summary>
        /// Resolves a path (relative to <see cref="FieldContainer"/> or absolute) as a data field.
        /// </summary>
        internal IDataField ResolveDataField(string path)
        {
            var (container, field) = FieldContainer.ResolveField(path);
            return ModuleData.CreateDataField(container, field);
        }

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
        internal int ReadInt32(ModuleOffset offset, int size)
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

        internal void WriteInt32(ModuleOffset offset, int size, int value)
        {
            ValidateRange(offset, size);
            if (ReadInt32(offset, size) == value)
            {
                return;
            }
            int start = offset.LogicalValue;
            switch (size)
            {
                case 1:
                    data[start] = (byte) value;
                    break;
                case 2:
                    data[start] = (byte) ((value >> 4) & 0xf);
                    data[1] = (byte) ((value >> 0) & 0xf);
                    break;
                case 4:
                    data[start] = (byte) ((value >> 12) & 0xf);
                    data[start + 1] = (byte) ((value >> 8) & 0xf);
                    data[start + 2] = (byte) ((value >> 4) & 0xf);
                    data[start + 3] = (byte) ((value >> 0) & 0xf);
                    break;
            }
            DataChanged?.Invoke(this, new DataChangedEventArgs(this, offset, offset + size));
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
            if (bytes.SequenceEqual(targetSpan))
            {
                return;
            }
            bytes.CopyTo(targetSpan);
            DataChanged?.Invoke(this, new DataChangedEventArgs(this, offset, offset + bytes.Length));
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
