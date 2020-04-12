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
        internal static FieldContainerData FromModel(Model.Data.FieldContainerData data) =>
            new FieldContainerData
            {
                Address = data.FieldContainer.Address.DisplayValue,
                Data = ByteString.CopyFrom(data.CopyData())
            };
    }
}
