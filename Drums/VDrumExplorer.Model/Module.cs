// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// A module consists of a <see cref="ModuleData"/> and
    /// the <see cref="ModuleSchema"/> that the data abides by.
    /// </summary>
    public sealed class Module
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }

        public Module(ModuleSchema schema, ModuleData data) =>
            (Schema, Data) = (schema, data);
    }
}
