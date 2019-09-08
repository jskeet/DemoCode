// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// An annotated field has a path from the root of the schema, along with a physical address.
    /// </summary>
    public sealed class AnnotatedField
    {
        public IField Field { get; }
        public string Path { get; }
        public ModuleAddress Address { get; }

        public AnnotatedField(string path, IField field, ModuleAddress address) =>
            (Path, Field, Address) = (path, field, address);
    }
}
