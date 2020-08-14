// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public abstract class NumericDataFieldBase : DataFieldBase<NumericFieldBase>
    {
        protected int Min => SchemaField.Min;
        protected int Max => SchemaField.Max;

        protected NumericDataFieldBase(NumericFieldBase schemaField) : base(schemaField)
        {
            rawValue = schemaField.Default;
        }

        protected override void RaisePropertyChanges()
        {
            base.RaisePropertyChanges();
            RaisePropertyChanged(nameof(RawValue));
        }

        private int rawValue;
        public int RawValue
        {
            get => rawValue;
            set
            {
                if (!TrySetRawValue(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Invalid raw value {value} for field {SchemaField.Path}");
                }
            }
        }

        public override void Reset() => RawValue = SchemaField.Default;

        internal override IEnumerable<DataValidationError> Load(DataSegment segment)
        {
            int rawValue = segment.ReadInt32(SchemaField.Offset, SchemaField.Codec);
            return TrySetRawValue(rawValue)
                ? DataValidationError.None
                : new[] { new DataValidationError(this, $"Invalid raw value {rawValue}") };
        }

        internal override void Save(DataSegment segment) =>
            segment.WriteInt32(SchemaField.Offset, SchemaField.Codec, RawValue);

        internal virtual bool TrySetRawValue(int value)
        {
            if (value < Min || value > Max)
            {
                return false;
            }
            SetProperty(ref rawValue, value);
            // Note: ignore the return value of SetProperty, as we have successfully
            // set the value. (The return value of SetProperty indicates a change.)
            return true;
        }
    }

    public abstract class NumericDataFieldBase<TField> : NumericDataFieldBase where TField : NumericFieldBase
    {
        public new TField SchemaField => (TField) base.SchemaField;

        protected NumericDataFieldBase(TField schemaField) : base(schemaField)
        {
        }
    }
}
