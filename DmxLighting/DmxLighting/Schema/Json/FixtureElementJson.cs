// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;

namespace DmxLighting.Schema.Json
{
    /// <summary>
    /// The JSON representation of a single element within a schema.
    /// </summary>
    internal class FixtureElementJson
    {
        /// <summary>
        /// Type of the element, e.g. "range", "enum", "switch", "placeholder".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Description of the element.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Dictionary from "name" to "lower value" for enums.
        /// </summary>
        public IDictionary<string, int> LowerValues { get; } = new Dictionary<string, int>();

        internal FixtureElement ToFixtureElement(int relativeChannel) => Type switch
        {
            "range" => new RangeElement(relativeChannel, Description),
            "enum" => new EnumElement(relativeChannel, Description, LowerValues),
            _ => throw new InvalidOperationException($"Unknown type '{Type}'")
        };
    }
}
