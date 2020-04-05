// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Proto
{
    internal partial class Module
    {
        internal Model.Module ToModel() =>
            new Model.Module(Identifier.GetSchema(), FieldContainerData.LoadData(Containers));

        internal static Module FromModel(Model.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Containers = { module.Data.Containers.Select(FieldContainerData.FromModel) }
            };
    }
}
