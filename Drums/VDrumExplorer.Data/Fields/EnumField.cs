// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        internal EnumField(FieldBase.Parameters common, IReadOnlyList<string> values)
            : base(common, 0, values.Count - 1) =>
            Values = values;

        public override string GetText(ModuleData data) => Values[GetRawValue(data)];

        public override bool TrySetText(ModuleData data, string text)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                if (Values[i] == text)
                {
                    SetRawValue(data, i);
                    return true;
                }
            }
            return false;
        }

        public void SetValue(ModuleData data, int value) => SetRawValue(data, value);
    }
}
