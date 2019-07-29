// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// An address in the compressed address sapace, which doesn't allow
    /// the high bit of any byte of the address to be set.
    /// This is pretty ugly - and potentially gives unexpected results -
    /// but means we don't need to check all over the place.
    /// </summary>
    public readonly struct ModuleAddress : IComparable<ModuleAddress>, IComparable, IEquatable<ModuleAddress>
    {
        public int Value { get; }

        public ModuleAddress(int value)
        {
            if ((value & 0x80808080L) != 0)
            {
                throw new ArgumentException($"Invalid address value: '{value:x}'");
            }
            Value = value;
        }

        /// <summary>
        /// Adds an offset to an address, compensating for the compressed address space
        /// where necessary by adding 0x80, 0x8000 or 0x800000.
        /// </summary>
        public static ModuleAddress operator +(ModuleAddress address, int offset)
        {
            int result = address.Value + offset;
            if ((result & 0x80) != 0)
            {
                result += 0x80;
            }
            if ((result & 0x80_00) != 0)
            {
                result += 0x80_00;
            }
            if ((result & 0x80_00_00) != 0)
            {
                result += 0x80_00_00;
            }
            return new ModuleAddress(result);
        }

        /// <summary>
        /// Subtracts an offset to an address, compensating for the compressed address space
        /// where necessary by adding 0x80, 0x8000 or 0x800000.
        /// </summary>
        public static ModuleAddress operator -(ModuleAddress address, int offset) =>
            address + (-offset);

        /// <summary>
        /// Returns the offset between two addresses.
        /// </summary>
        public static int operator -(ModuleAddress lhs, ModuleAddress rhs) =>
            lhs.Value - rhs.Value;

        public override string ToString() => Value.ToString("x8");

        public int CompareTo(ModuleAddress other) => Value.CompareTo(other.Value);

        public int CompareTo(object obj) =>
            obj is ModuleAddress other
            ? CompareTo(other)
            : throw new ArgumentException($"Cannot compare {nameof(ModuleAddress)} with {obj?.GetType().Name ?? "null"}");

        public bool Equals(ModuleAddress other) => Value == other.Value;

        public override int GetHashCode() => Value;

        public override bool Equals(object? obj) => obj is ModuleAddress other && Equals(other);
    }
}
