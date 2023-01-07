// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Linq;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class Kit
    {
        internal Model.Kit ToModel(ILogger logger)
        {
            var data = Containers.Select(fcd => fcd.ToModel());
            var snapshot = new ModuleDataSnapshot();
            foreach (var container in Containers)
            {
                snapshot.Add(container.ToModel());
            }
            var schema = Identifier.GetOrInferSchema(schema => snapshot.IsValidForNode(schema.Kit1Root), logger);
            return Model.Kit.FromSnapshot(schema, snapshot, DefaultKitNumber == 0 ? 1 : DefaultKitNumber, logger);
        }

        internal static Kit FromModel(Model.Kit kit) =>
            new Kit
            {
                Identifier = ModuleIdentifier.FromModel(kit.Schema.Identifier),
                Containers = { kit.Data.CreateSnapshot().Segments.Select(FieldContainerData.FromModel) },
                DefaultKitNumber = kit.DefaultKitNumber
            };
    }
}
