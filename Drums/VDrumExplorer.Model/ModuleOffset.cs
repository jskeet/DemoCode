// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// An offset between <see cref="ModuleAddress"/> values. These are represented in
    /// the display address space.
    /// </summary>
    public readonly struct ModuleOffset : IEquatable<ModuleOffset>
    {
        public static ModuleOffset Zero { get; } = new ModuleOffset(0);

        public int LogicalValue { get; }
        public int DisplayValue { get; }

        private ModuleOffset(int logicalValue)
        {
            LogicalValue = logicalValue;
            DisplayValue =
                ((logicalValue & 0b1111111) << 0) |
                ((logicalValue & 0b1111111_0000000) << 1) |
                ((logicalValue & 0b1111111_0000000_0000000) << 2) |
                ((logicalValue & 0b1111111_0000000_0000000_0000000) << 3);
        }

        /// <summary>
        /// Returns an offset based on a display value, representing an address space where
        /// the top bit of each byte is always clear.
        /// </summary>
        public static ModuleOffset FromDisplayValue(int displayValue)
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
            return new ModuleOffset(logicalValue);
        }

        public static ModuleOffset operator +(ModuleOffset offset, int logicalGap) =>
            new ModuleOffset(offset.LogicalValue + logicalGap);

        public override string ToString() => DisplayValue.ToString("x8");

        public bool Equals(ModuleOffset other) => DisplayValue == other.DisplayValue;

        public override int GetHashCode() => DisplayValue;

        public override bool Equals(object? obj) => obj is ModuleAddress other && Equals(other);
    }
}
