// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class Module
    {
        internal Model.Module ToModel()
        {
            var snapshot = new ModuleDataSnapshot();
            foreach (var container in Containers)
            {
                snapshot.Add(container.ToModel());
            }

            return Model.Module.Create(Identifier.GetSchema(), snapshot);
        }

        internal static Module FromModel(Model.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Containers = { module.Data.CreateSnapshot().Segments.Select(FieldContainerData.FromModel) }
            };
    }
}
