// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using BenchmarkDotNet.Attributes;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Benchmarks
{
    [MemoryDiagnoser]
    public class SchemaBenchmarks
    {
        [Benchmark]
        public ModuleSchema LoadTD27Schema() =>
            ModuleSchema.FromAssemblyResources(typeof(ModuleSchema).Assembly, "TD27", "TD27.json", ModuleIdentifier.TD27.SoftwareRevision);
    }
}
