// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Data.Proto
{
    internal partial class Module
    {
        internal Data.Module ToModel() =>
            new Data.Module(Identifier.GetSchema(), DataSegment.LoadData(Segments));

        internal static Module FromModel(Data.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Segments = { module.Data.GetSegments().Select(DataSegment.FromModel) }
            };
    }
}
