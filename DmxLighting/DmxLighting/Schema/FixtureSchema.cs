// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;
using DmxLighting.Schema.Json;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DmxLighting.Schema
{
    /// <summary>
    /// The schema for a DMX-controlled lighting fixture. If
    /// a fixture has multiple modes, each should be represented
    /// by a separate schema.
    /// </summary>
    public class FixtureSchema
    {
        /// <summary>
        /// Identifier to be used in configuration files etc
        /// that refer to this schema. Ideally should be human-recognizable,
        /// but also compact.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name to display in user interfaces. This can be longer
        /// than the ID, and may be changed in schema definitions
        /// later (as it shouldn't be stored or used in configuration files
        /// etc).
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The number of DMX channels to reserve for this fixture,
        /// even if not all of them are currently represented.
        /// </summary>
        public int Channels { get; }

        public IReadOnlyList<FixtureElementGroup> Groups { get; }

        internal FixtureSchema(string id, string displayName, int channels, IEnumerable<FixtureElementGroup> groups)
        {
            Id = id;
            DisplayName = displayName;
            Channels = channels;
            Groups = groups.ToList().AsReadOnly();
        }

        public static FixtureSchema FromJsonResource(string resourceName)
        {
            using var stream = typeof(FixtureSchema).Assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            var schemaJson = JsonConvert.DeserializeObject<FixtureSchemaJson>(json);
            return schemaJson.ToFixtureSchema();
        }

        public FixtureData ToFixtureData(DmxUniverse universe, int fixtureFirstChannel) =>
            new FixtureData(this, Groups.Select(group => group.ToGroupData(universe, fixtureFirstChannel)));
    }
}
