// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        internal EnumField(FieldBase.Parameters common, IReadOnlyList<string> values, int min, int @default)
            : base(common, min, values.Count + min - 1, @default) =>
            Values = values;

        public override string GetText(FixedContainer context, ModuleData data) => Values[GetRawValue(context, data) - Min];

        public override bool TrySetText(FixedContainer context, ModuleData data, string text)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                if (Values[i] == text)
                {
                    SetRawValue(context, data, i + Min);
                    return true;
                }
            }
            return false;
        }

        public void SetValue(FixedContainer context, ModuleData data, int value) => SetRawValue(context, data, value + Min);
    }
}
