// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace DmxLighting.Schema.Json
{
    internal class FixtureElementGroupJson
    {
        /// <summary>
        /// Description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Elements in the group.
        /// </summary>
        public List<FixtureElementJson> Elements { get; } = new List<FixtureElementJson>();

        internal FixtureElementGroup ToFixtureElementGroup(ref int relativeChannel)
        {
            var convertedElements = new List<FixtureElement>();
            foreach (var element in Elements)
            {
                var converted = element.ToFixtureElement(relativeChannel);
                relativeChannel++;
                convertedElements.Add(converted);
            }
            return new FixtureElementGroup(Description, convertedElements);
        }
    }
}
