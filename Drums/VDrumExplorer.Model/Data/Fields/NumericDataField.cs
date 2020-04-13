// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Fields;
using static System.FormattableString;

namespace VDrumExplorer.Model.Data.Fields
{
    public class NumericDataField : NumericDataFieldBase<NumericField>
    {
        internal NumericDataField(FieldContainerData context, NumericField field) : base(context, field)
        {
        }

        protected override void OnDataChanged() => RaisePropertyChange(nameof(RawValue));

        public override string FormattedText => GetText();

        public string GetText()
        {
            int value = RawValue;
            if (SchemaField.CustomValueFormatting is (int customValue, string text) && value == customValue)
            {
                return text;
            }
            decimal scaled = ScaleRawValueForFormatting(value);
            return Invariant($"{scaled}{SchemaField.Suffix}");
        }

        private decimal ScaleRawValueForFormatting(int value)
        {
            value += SchemaField.ValueOffset ?? 0;
            value *= SchemaField.Multiplier ?? 1;
            return value / (SchemaField.Divisor ?? 1m);
        }

    }
}
