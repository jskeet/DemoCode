// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
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

        public Module(ModuleData data) =>
            (Schema, Data) = (data.PhysicalRoot.Schema, data);

        public static Module Create(ModuleSchema moduleSchema, Dictionary<ModuleAddress, byte[]> data)
        {
            var moduleData = ModuleData.FromData(moduleSchema.LogicalRoot, data);
            return new Module(moduleData);
        }
    }
}
