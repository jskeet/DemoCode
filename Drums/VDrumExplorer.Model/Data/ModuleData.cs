// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.PhysicalSchema;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// The data for a module. (Very much a work in progress...)
    /// </summary>
    public sealed class ModuleData
    {
        private readonly Dictionary<ModuleAddress, FieldContainerData> containers = new Dictionary<ModuleAddress, FieldContainerData>();

        public IEnumerable<FieldContainerData> Containers => containers.Values.OrderBy(c => c.Address);

        public void AddContainer(FieldContainerData container)
        {
            containers.Add(container.Address, container);
        }

        public FieldContainerData GetContainerData(FieldContainer container) => containers[container.Address];
    }
}
