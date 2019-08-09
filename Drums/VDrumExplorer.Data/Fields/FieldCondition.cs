// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public class FieldCondition
    {
        public ModuleAddress ReferenceAddress { get; }
        public int RequiredValue { get; }

        public FieldCondition(ModuleAddress referenceAddress, int requiredValue) =>
            (ReferenceAddress, RequiredValue) = (referenceAddress, requiredValue);

        public bool IsEnabled(IField field, ModuleData data)
        {
            // We can't just ask the schema for the field at the reference address, because
            // it might be within an overlay. We could potentially simplify things by making each field
            // know the container it's part of, and making the condition know which field *it's* part of.
            var container = field.Schema.ParentsByField[field];
            var referenceField = (NumericFieldBase)container.FieldsByAddress[ReferenceAddress];
            int value = referenceField.GetRawValue(data);
            return value == RequiredValue;
        }
    }
}
