// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;

namespace VDrumExplorer.Data.Proto
{
    internal partial class DataSegment
    {
        internal Data.DataSegment ToModel() =>
            new Data.DataSegment(new ModuleAddress(Start), Data.ToByteArray());

        internal static DataSegment FromModel(Data.DataSegment segment) =>
            new DataSegment
            {
                Start = segment.Start.Value,
                Data = ByteString.CopyFrom(segment.CopyData())
            };
    }
}
