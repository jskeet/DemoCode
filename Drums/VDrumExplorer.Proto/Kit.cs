// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Linq;

namespace VDrumExplorer.Proto
{
    internal partial class Kit
    {
        internal Model.Kit ToModel() =>
            new Model.Kit(Identifier.GetSchema(), FieldContainerData.LoadData(Containers), DefaultKitNumber == 0 ? 1 : DefaultKitNumber);

        internal static Kit FromModel(Model.Kit kit) =>
            new Kit
            {
                Identifier = ModuleIdentifier.FromModel(kit.Schema.Identifier),
                Containers = { kit.Data.Containers.Select(FieldContainerData.FromModel) },
                DefaultKitNumber = kit.KitNumber
            };
    }
}
