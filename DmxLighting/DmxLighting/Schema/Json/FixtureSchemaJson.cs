// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace DmxLighting.Schema.Json
{
    /// <summary>
    /// The JSON representation of a complete fixture schema.
    /// </summary>
    internal class FixtureSchemaJson
    {
        /// <summary>
        /// ID of the schema.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the schema.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The number of DMX channels to reserve for this fixture,
        /// even if not all of them are currently represented.
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// Groups within the schema. These are just for display purposes, effectively.
        /// </summary>
        public List<FixtureElementGroupJson> Groups { get; } = new List<FixtureElementGroupJson>();

        internal FixtureSchema ToFixtureSchema()
        {
            int relativeChannel = 0;
            List<FixtureElementGroup> convertedGroups = new List<FixtureElementGroup>();
            foreach (var group in Groups)
            {
                convertedGroups.Add(group.ToFixtureElementGroup(ref relativeChannel));
            }
            return new FixtureSchema(Id, DisplayName, Channels, convertedGroups);
        }
    }
}