﻿using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        public EnumField(FieldPath path, ModuleAddress address, int size, string description, IReadOnlyList<string> values)
            : base(path, address, size, description, 0, values.Count - 1) =>
            Values = values;

        public override string GetText(ModuleData data) => Values[GetRawValue(data)];
    }
}
