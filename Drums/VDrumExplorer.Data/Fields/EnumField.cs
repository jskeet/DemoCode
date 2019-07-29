// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        public EnumField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition, IReadOnlyList<string> values)
            : base(path, address, size, description, condition, 0, values.Count - 1) =>
            Values = values;

        public override string GetText(ModuleData data) => Values[GetRawValue(data)];
    }
}
