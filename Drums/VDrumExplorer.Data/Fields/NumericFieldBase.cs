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
    public abstract class NumericFieldBase : PrimitiveFieldBase, IPrimitiveField
    {
        public int Min { get; }
        public int Max { get; }
        public int Default { get; }

        private protected NumericFieldBase(FieldBase.Parameters common, int min, int max, int @default)
            : base(common) =>
            (Min, Max, Default) = (min, max, @default);

        private int GetRawValueUnvalidated(FixedContainer context, ModuleData data)
        {
            var address = GetAddress(context);
            return Size switch
            {
                1 => data.GetAddressValue(address),
                2 => ((sbyte) ((data.GetAddressValue(address) << 4) | data.GetAddressValue(address + 1))),
                // TODO: Just fetch a byte array? Stackalloc it?
                4 => (short) (
                    (data.GetAddressValue(address) << 12) |
                    (data.GetAddressValue(address + 1) << 8) |
                    (data.GetAddressValue(address + 2) << 4) |
                    data.GetAddressValue(address + 3)),
                _ => throw new InvalidOperationException($"Cannot get value with size {Size}")
            };
        }

        internal int GetRawValue(FixedContainer context, ModuleData data)
        {
            var value = GetRawValueUnvalidated(context, data);
            if (value < Min || value > Max)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            return value;
        }

        internal void SetRawValue(FixedContainer context, ModuleData data, int newValue)
        {
            if (newValue < Min || newValue > Max)
            {
                throw new ArgumentOutOfRangeException($"Value out of range. Min={Min}; Max={Max}; Attempt={newValue}");
            }
            byte[] bytes = new byte[Size];
            switch (Size)
            {
                case 1:
                    bytes[0] = (byte) newValue;
                    break;
                case 2:
                    bytes[0] = (byte) ((newValue >> 4) & 0xf);
                    bytes[1] = (byte) ((newValue >> 0) & 0xf);
                    break;
                case 4:
                    bytes[0] = (byte) ((newValue >> 12) & 0xf);
                    bytes[1] = (byte) ((newValue >> 8) & 0xf);
                    bytes[2] = (byte) ((newValue >> 4) & 0xf);
                    bytes[3] = (byte) ((newValue >> 0) & 0xf);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot set value with size {Size}");
            }
            data.SetData(GetAddress(context), bytes);
        }

        public override void Reset(FixedContainer context, ModuleData data) => SetRawValue(context, data, Default);

        protected override bool ValidateData(FixedContainer context, ModuleData data, out string? error)
        {
            var rawValue = GetRawValueUnvalidated(context, data);
            if (rawValue < Min || rawValue > Max)
            {
                error = $"Invalid raw value {rawValue}. Expected range: [{Min}-{Max}]";
                return false;
            }
            error = null;
            return true;
        }
    }
}
