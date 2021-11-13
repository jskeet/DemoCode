// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;
using System.Collections.Generic;
using System.Linq;

namespace DmxLighting.Schema
{
    public sealed class FixtureElementGroup
    {
        public string Description { get; }
        public IReadOnlyList<FixtureElement> Elements { get; }

        public FixtureElementGroup(string description, IEnumerable<FixtureElement> elements)
        {
            Description = description;
            Elements = elements.ToList().AsReadOnly();
        }

        internal GroupData ToGroupData(DmxUniverse universe, int fixtureFirstChannel) =>
            new GroupData(Description, Elements.Select(e => e.ToElementData(universe, fixtureFirstChannel)));
    }
}
