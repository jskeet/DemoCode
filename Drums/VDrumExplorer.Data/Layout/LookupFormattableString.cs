// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    internal sealed class LookupFormattableString : IModuleDataFormattableString
    {
        public FieldPath Path { get; }
        public string Value { get; }
        public ModuleAddress? Address => null;

        public LookupFormattableString(FieldPath path, string value) => (Path, Value) = (path, value);

        public string Format(ModuleData data) => Value;

        public override string ToString() => $"{Path}: {Value}";
    }
}
