// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// Abstract base class for fields based on a numeric value in a given range.
    /// Concrete subclasses provide formatting.
    /// </summary>
    public abstract class NumericFieldBase : FieldBase, IPrimitiveField
    {
        public int Min { get; }
        public int Max { get; }

        private protected NumericFieldBase(FieldBase.Parameters common, int min, int max)
            : base(common) =>
            (Min, Max) = (min, max);

        public abstract string GetText(ModuleData data);
        public abstract bool TrySetText(ModuleData data, string text);

        internal int GetRawValue(ModuleData data)
        {
            int value = Size switch
            {
                1 => data.GetAddressValue(Address),
                2 => ((sbyte)((data.GetAddressValue(Address) << 4) | data.GetAddressValue(Address + 1))),
                        // TODO: Just fetch a byte array? Stackalloc it?
                4 => (short)(
                    (data.GetAddressValue(Address) << 12) |
                    (data.GetAddressValue(Address + 1) << 8) |
                    (data.GetAddressValue(Address + 2) << 4) |
                    data.GetAddressValue(Address + 3)),
                _ => throw new InvalidOperationException($"Cannot get value with size {Size}")
            };
            
            if (value < Min || value > Max)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            return value;
        }

        internal void SetRawValue(ModuleData data, int newValue)
        {
            if (newValue < Min || newValue > Max)
            {
                throw new ArgumentOutOfRangeException($"Value out of range. Min={Min}; Max={Max}; Attempt={newValue}");
            }
            switch (Size)
            {
                case 1:
                    data.SetAddressValue(Address, (byte) newValue);
                    break;
                case 2:
                    data.SetAddressValue(Address, (byte) ((newValue >> 4) & 0xf));
                    data.SetAddressValue(Address + 1, (byte) ((newValue >> 0) & 0xf));
                    break;
                case 4:
                    data.SetAddressValue(Address, (byte) ((newValue >> 12) & 0xf));
                    data.SetAddressValue(Address + 1, (byte) ((newValue >> 8) & 0xf));
                    data.SetAddressValue(Address + 2, (byte) ((newValue >> 4) & 0xf));
                    data.SetAddressValue(Address + 3, (byte) ((newValue >> 0) & 0xf));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot set value with size {Size}");
            }
        }
    }
}
