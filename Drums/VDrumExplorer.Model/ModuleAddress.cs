// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// An address in module data, with representations in logical address space (contiguous)
    /// or display address space (7-bit addressing, so the top bit of each byte is always clear).
    /// </summary>
    public readonly struct ModuleAddress : IComparable<ModuleAddress>, IComparable, IEquatable<ModuleAddress>
    {
        public int DisplayValue { get; }
        public int LogicalValue { get; }

        private ModuleAddress(int logicalValue)
        {
            LogicalValue = logicalValue;
            DisplayValue =
                ((logicalValue & 0b1111111) << 0) |
                ((logicalValue & 0b1111111_0000000) << 1) |
                ((logicalValue & 0b1111111_0000000_0000000) << 2) |
                ((logicalValue & 0b1111111_0000000_0000000_0000000) << 3);
        }

        /// <summary>
        /// Returns an address based on a logical value, representing a contiguous address space.
        /// </summary>
        public static ModuleAddress FromLogicalValue(int logicalValue)
        {
            if (logicalValue < 0 || logicalValue >= 1 << 28)
            {
                throw new ArgumentException($"Invalid address logical value: '{logicalValue:x8}'");
            }
            return new ModuleAddress(logicalValue);
        }

        /// <summary>
        /// Returns an address advanced by the given logical offset.
        /// </summary>
        public ModuleAddress PlusLogicalOffset(int logicalOffset) => new ModuleAddress(LogicalValue + logicalOffset);

        /// <summary>
        /// Returns an address based on a display value, representing an address space where the top
        /// bit of each byte is always clear.
        /// </summary>
        public static ModuleAddress FromDisplayValue(int displayValue)
        {
            if ((displayValue & 0x80808080L) != 0)
            {
                throw new ArgumentException($"Invalid address display value: '{displayValue:x8}'");
            }
            int logicalValue = 
                ((displayValue & 0x7f_00_00_00) >> 3) |
                ((displayValue & 0x00_7f_00_00) >> 2) |
                ((displayValue & 0x00_00_7f_00) >> 1) |
                ((displayValue & 0x00_00_00_7f) >> 0);
            return new ModuleAddress(logicalValue);
        }

        /// <summary>
        /// Adds an offset to an address, compensating for the compressed address space
        /// where necessary by adding 0x80, 0x8000 or 0x800000.
        /// </summary>
        public static ModuleAddress operator +(ModuleAddress address, ModuleOffset offset)
        {
            int newDisplayValue = address.DisplayValue + offset.DisplayValue;
            // TODO: Eliminate this if possible. It's a bit annoying. We should at least work out where it's happening.
            if ((newDisplayValue & 0x80) != 0)
            {
                newDisplayValue += 0x80;
            }
            if ((newDisplayValue & 0x80_00) != 0)
            {
                newDisplayValue += 0x80_00;
            }
            if ((newDisplayValue & 0x80_00_00) != 0)
            {
                newDisplayValue += 0x80_00_00;
            }
            return FromDisplayValue(newDisplayValue);
        }

        public override string ToString() => DisplayValue.ToString("x8");

        public int CompareTo(ModuleAddress other) => LogicalValue.CompareTo(other.LogicalValue);

        public int CompareTo(object obj) =>
            obj is ModuleAddress other
            ? CompareTo(other)
            : throw new ArgumentException($"Cannot compare {nameof(ModuleAddress)} with {obj?.GetType().Name ?? "null"}");

        public bool Equals(ModuleAddress other) => LogicalValue == other.LogicalValue;

        public override int GetHashCode() => LogicalValue;

        public override bool Equals(object? obj) => obj is ModuleAddress other && Equals(other);
    }
}
