// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class BooleanField : EnumField
    {
        private static readonly IReadOnlyList<string> OffOnValues = new List<string> { "Off", "On" }.AsReadOnly();

        internal BooleanField(FieldBase.Parameters common)
            : base(common, OffOnValues)
        {
        }

        public bool GetValue(ModuleData data) => GetRawValue(data) == 1;

        public void SetValue(ModuleData data, bool value) => SetRawValue(data, value ? 1 : 0);
    }
}
