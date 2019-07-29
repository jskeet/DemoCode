// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;

namespace VDrumExplorer.Data
{
    // TODO: Not really sure what to do with this, static/instance methods, non-public etc.
    public static class SchemaRegistry
    {
        // The laziness is primarily to avoid loading data within a type initializer, which causes all kinds of problems.
        private static readonly Lazy<ModuleSchema> td17 =
            new Lazy<ModuleSchema>(() => ModuleSchema.FromAssemblyResources(typeof(SchemaRegistry).Assembly, "VDrumExplorer.Data.TD17", "TD17.json"));

        public static IReadOnlyList<ModuleSchema> GetSchemas() =>
            new List<ModuleSchema>
            {
                td17.Value
            }
            .AsReadOnly();
    }
}
