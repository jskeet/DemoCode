// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    internal sealed class FieldFormattableString : IModuleDataFormattableString
    {
        private IPrimitiveField field;

        public FieldFormattableString(IPrimitiveField field) => this.field = field;

        public ModuleAddress? Address => field.Address;

        public string Format(ModuleData data) => field.GetText(data);
    }
}
