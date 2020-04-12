// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;
using VDrumExplorer.Model;

namespace VDrumExplorer.Proto
{
    internal partial class Kit
    {
        internal Model.Kit ToModel()
        {
            var data = Containers.ToDictionary(
                container => ModuleAddress.FromDisplayValue(container.Address),
                container => container.Data.ToByteArray());
            return Model.Kit.Create(Identifier.GetSchema(), data, DefaultKitNumber == 0 ? 1 : DefaultKitNumber);
        }

        internal static Kit FromModel(Model.Kit kit) =>
            new Kit
            {
                Identifier = ModuleIdentifier.FromModel(kit.Schema.Identifier),
                Containers = { kit.Data.Containers.Select(FieldContainerData.FromModel) },
                DefaultKitNumber = kit.KitNumber
            };
    }
}
