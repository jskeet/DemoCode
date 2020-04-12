// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class Module
    {
        internal Model.Module ToModel()
        {
            var data = Containers.ToDictionary(
                container => ModuleAddress.FromDisplayValue(container.Address),
                container => container.Data.ToByteArray());
            return Model.Module.Create(Identifier.GetSchema(), data);
        }

        internal static Module FromModel(Model.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Containers = { module.Data.Containers.Select(FieldContainerData.FromModel) }
            };
    }
}
