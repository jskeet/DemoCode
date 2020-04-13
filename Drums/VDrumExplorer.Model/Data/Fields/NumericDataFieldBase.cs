// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public abstract class NumericDataFieldBase<TField> : DataFieldBase<TField> where TField : NumericFieldBase
    {
        protected int Min => SchemaField.Min;
        protected int Max => SchemaField.Max;

        protected NumericDataFieldBase(FieldContainerData context, TField schemaField)
            : base(context, schemaField)
        {
        }

        protected override void OnDataChanged()
        {
            RaisePropertyChange(nameof(RawValue));
        }

        public int RawValue
        {
            get => GetRawValue();
            set
            {
                if (!TrySetRawValue(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        protected int GetRawValueUnvalidated() => Context.ReadInt32(Offset, Size);

        internal int GetRawValue()
        {
            var value = GetRawValueUnvalidated();
            if (value < Min || value > Max)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            return value;
        }

        internal bool TrySetRawValue(int value)
        {
            if (value < Min || value > Max)
            {
                return false;
            }
            Context.WriteInt32(Offset, Size, value);
            return true;
        }

        internal bool ValidateData(out string? error)
        {
            var rawValue = GetRawValueUnvalidated();
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
