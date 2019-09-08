// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    public class FieldCondition
    {
        public int ReferenceOffset { get; }
        public int RequiredValue { get; }

        public FieldCondition(int referenceOffset, int requiredValue) =>
            (ReferenceOffset, RequiredValue) = (referenceOffset, requiredValue);

        public bool IsEnabled(IField field, FixedContainer context, ModuleData data)
        {
            // This is ugly and slow - it would be great not to have to iterate like this...
            var referenceField = context.GetPrimitiveFields(data).OfType<NumericFieldBase>().Single(field => field.Offset == ReferenceOffset);
            int value = referenceField.GetRawValue(context, data);
            return value == RequiredValue;
        }
    }
}
