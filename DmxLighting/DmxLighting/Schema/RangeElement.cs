// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;

namespace DmxLighting.Schema
{
    public class RangeElement : FixtureElement
    {
        public RangeElement(int relativeChannel, string description) : base(relativeChannel, description)
        {
        }

        internal override ElementData ToElementData(DmxUniverse universe, int fixtureFirstChannel) =>
            new RangeElementData(universe, fixtureFirstChannel + RelativeChannel, this);

    }
}
