// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Proto
{
    internal partial class Module
    {
        internal Model.Module ToModel()
        {
            var data = Containers.Select(fcd => fcd.ToModel());
            return Model.Module.Create(Identifier.GetSchema(), data);
        }

        internal static Module FromModel(Model.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Containers = { module.Data.SerializeData().Select(FieldContainerData.FromModel) }
            };
    }
}
