﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
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
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public override void Reset() => RawValue = SchemaField.Default;

        // TODO: Validation? Currently just throws.
        internal override void Load(DataSegment segment) =>
            RawValue = segment.ReadInt32(SchemaField.Offset, SchemaField.Size);

        internal override void Save(DataSegment segment) =>
            segment.WriteInt32(SchemaField.Offset, SchemaField.Size, RawValue);

        internal bool TrySetRawValue(int value)
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
