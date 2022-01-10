// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DmxLighting.Schema
{
    public class EnumElement : FixtureElement
    {
        public IReadOnlyList<string> Names { get; }

        private readonly IReadOnlyDictionary<string, byte> lowerValuesByName;

        private readonly ValueRange[] rangesByValue;

        public EnumElement(int relativeChannel, string description, IDictionary<string, int> lowerValues) : base(relativeChannel, description)
        {
            lowerValuesByName = new ReadOnlyDictionary<string, byte>(lowerValues.ToDictionary(pair => pair.Key, pair => (byte) pair.Value));

            var lowerValuesInOrder = lowerValuesByName.OrderBy(pair => pair.Value).ToList();
            if (lowerValuesInOrder[0].Value != 0)
            {
                throw new ArgumentException($"Invalid value for element {description}: no value for 0");
            }
            rangesByValue = new ValueRange[256];
            for (int i = 0; i < lowerValuesInOrder.Count; i++)
            {
                var upperBound = (byte) (i == lowerValuesInOrder.Count - 1 ? 255 : lowerValuesInOrder[i + 1].Value - 1);
                var range = new ValueRange(lowerValuesInOrder[i].Value, upperBound, lowerValuesInOrder[i].Key);
                for (int j = range.LowerBoundInclusive; j <= range.UpperBoundInclusive; j++)
                {
                    rangesByValue[j] = range;
                }
            }

            Names = lowerValuesInOrder.Select(pair => pair.Key).ToList().AsReadOnly();
        }

        public ValueRange GetRange(byte value) => rangesByValue[value];

        public byte GetLowerValue(string name) => lowerValuesByName[name];

        internal override ElementData ToElementData(DmxUniverse universe, int fixtureFirstChannel) =>
            new EnumElementData(universe, fixtureFirstChannel + RelativeChannel, this);

        public class ValueRange
        {
            public byte LowerBoundInclusive { get; }
            public byte UpperBoundInclusive { get; }
            public string Name { get; }

            public ValueRange(byte lowerBoundInclusive, byte upperBoundInclusive, string name) =>
                (LowerBoundInclusive, UpperBoundInclusive, Name) =
                (lowerBoundInclusive, upperBoundInclusive, name);
        }
    }
}
