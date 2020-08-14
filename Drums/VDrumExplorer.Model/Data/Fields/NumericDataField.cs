// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public sealed class NumericDataField : NumericDataFieldBase<NumericField>
    {
        internal NumericDataField(NumericField field) : base(field)
        {
        }

        public override string FormattedText => SchemaField.FormatRawValue(RawValue);
    }
}
