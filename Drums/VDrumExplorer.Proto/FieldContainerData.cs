// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;
using Google.Protobuf.Collections;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class FieldContainerData
    {
        internal Model.Data.FieldContainerData ToModel() =>
            new Model.Data.FieldContainerData(Model.ModuleAddress.FromDisplayValue(Address), Data.ToByteArray());

        internal static FieldContainerData FromModel(Model.Data.FieldContainerData container) =>
            new FieldContainerData
            {
                Address = container.Address.DisplayValue,
                Data = ByteString.CopyFrom(container.CopyData())
            };

        internal static ModuleData LoadData(RepeatedField<FieldContainerData> containers)
        {
            var moduleData = new ModuleData();
            foreach (var container in containers)
            {
                moduleData.AddContainer(container.ToModel());
            }
            return moduleData;
        }
    }
}
