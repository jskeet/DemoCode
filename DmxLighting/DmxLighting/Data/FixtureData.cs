// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Schema;
using System.Collections.Generic;
using System.Linq;

namespace DmxLighting.Data
{
    public class FixtureData
    {
        public FixtureSchema Schema { get; }
        public IReadOnlyList<GroupData> Groups { get; }

        public FixtureData(FixtureSchema schema, IEnumerable<GroupData> groups) =>
            (Schema, Groups) = (schema, groups.ToList().AsReadOnly());

    }
}
