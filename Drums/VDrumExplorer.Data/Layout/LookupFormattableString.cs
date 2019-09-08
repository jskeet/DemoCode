// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    internal sealed class LookupFormattableString : IModuleDataFormattableString
    {
        public string Path { get; }
        public string Value { get; }

        public LookupFormattableString(string path, string value) => (Path, Value) = (path, value);

        public string Format(FixedContainer context, ModuleData data) => Value;

        public override string ToString() => $"{Path}: {Value}";

        public ModuleAddress? GetSegmentAddress(FixedContainer context) => null;
    }
}
