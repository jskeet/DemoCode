// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public class BooleanDataField : NumericDataFieldBase<BooleanField>
    {
        internal BooleanDataField(BooleanField field) : base(field)
        {
        }

        protected override void RaisePropertyChanges()
        {
            base.RaisePropertyChanges();
            RaisePropertyChanged(nameof(Value));
        }

        public bool Value
        {
            get => RawValue == 1;
            set => RawValue = value ? 1 : 0;
        }

        public override string FormattedText => Value ? "On" : "Off";
    }
}
