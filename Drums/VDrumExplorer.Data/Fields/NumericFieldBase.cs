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
        public int? Min { get; }
        public int? Max { get; }

        public NumericFieldBase(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition, int min, int max)
            : base(path, address, size, description, condition) =>
            (Min, Max) = (min, max);

        public abstract string GetText(ModuleData data);

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
    }
}
