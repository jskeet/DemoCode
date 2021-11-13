// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DmxLighting.Schema
{
    public class EnumElement : FixtureElement
    {
        public IReadOnlyList<string> Names { get; }

        private readonly IReadOnlyDictionary<string, byte> lowerValuesByName;

        public EnumElement(int relativeChannel, string description, IDictionary<string, int> lowerValues) : base(relativeChannel, description)
        {
            lowerValuesByName = new ReadOnlyDictionary<string, byte>(lowerValues.ToDictionary(pair => pair.Key, pair => (byte) pair.Value));
            Names = lowerValuesByName.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList().AsReadOnly();
        }

        public byte GetLowerValue(string name) => lowerValuesByName[name];

        public string GetName(byte value)
        {
            string previousName = null;
            // TODO: Make this more efficient if we need to.
            foreach (var name in Names)
            {
                if (value < GetLowerValue(name))
                {
                    return previousName;
                }
                previousName = name;
            }
            return previousName;
        }

        internal override ElementData ToElementData(DmxUniverse universe, int fixtureFirstChannel) =>
            new EnumElementData(universe, fixtureFirstChannel + RelativeChannel, this);
    }
}
