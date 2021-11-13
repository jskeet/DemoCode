// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;

namespace DmxLighting.Schema
{
    public abstract class FixtureElement
    {
        protected FixtureElement(int relativeChannel, string description)
        {
            RelativeChannel = relativeChannel;
            Description = description;
        }

        /// <summary>
        /// DMX channel of this element, relative to the start of the schema.
        /// The first element has a RelativeChannel of 0, for example.
        /// </summary>
        public int RelativeChannel { get; }

        /// <summary>
        /// A human-readable description of the element.
        /// </summary>
        public string Description { get; }

        internal abstract ElementData ToElementData(DmxUniverse universe, int fixtureFirstChannel);
    }
}
