// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        public StringField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition)
            : base(path, address, size, description, condition)
        {
        }

        public string GetText(ModuleData data)
        {
            byte[] bytes = data.GetData(Address, Size);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
