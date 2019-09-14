// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Data.Proto
{
    internal partial class Kit
    {
        internal Data.Kit ToModel() =>
            new Data.Kit(Identifier.GetSchema(), DataSegment.LoadData(Segments));

        internal static Kit FromModel(Data.Kit kit) =>
            new Kit
            {
                Identifier = ModuleIdentifier.FromModel(kit.Schema.Identifier),
                Segments = { kit.Data.GetSegments().Select(DataSegment.FromModel) }
            };
    }
}
