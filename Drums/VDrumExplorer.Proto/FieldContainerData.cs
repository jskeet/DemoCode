// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    internal partial class FieldContainerData
    {
        internal static FieldContainerData FromModel(DataSegment data) =>
            new FieldContainerData
            {
                Address = data.Address.DisplayValue,
                Data = ByteString.CopyFrom(data.CopyData())
            };

        internal DataSegment ToModel() =>
            new DataSegment(ModuleAddress.FromDisplayValue(Address), Data.ToByteArray());
    }
}
