// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Linq;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class Module
    {
        internal Model.Module ToModel(ILogger logger)
        {
            var snapshot = new ModuleDataSnapshot();
            foreach (var container in Containers)
            {
                snapshot.Add(container.ToModel());
            }
            var schema = Identifier.GetOrInferSchema(schema => snapshot.IsValidForNode(schema.LogicalRoot), logger);
            return Model.Module.FromSnapshot(schema, snapshot, logger);
        }

        internal static Module FromModel(Model.Module module) =>
            new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Containers = { module.Data.CreateSnapshot().Segments.Select(FieldContainerData.FromModel) }
            };
    }
}
