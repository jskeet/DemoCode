// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.Schema.Json
{
    /// <summary>
    /// The JSON representation of information about a dynamic overlay.
    /// </summary>
    internal sealed class DynamicOverlayJson
    {
        /// <summary>
        /// The path of the field to switch on.
        /// </summary>
        public string? SwitchPath { get; set; }

        /// <summary>
        /// The size of each field in the overlay.
        /// </summary>
        public int FieldSize { get; set; }

        /// <summary>
        /// The number of fields in the overlay.
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// The lists of overlaid fields to switch between.
        /// </summary>
        public Dictionary<string, OverlaidFieldListJson>? FieldLists { get; set; }

        internal sealed class OverlaidFieldListJson
        {
            public List<string>? Order { get; set; }
            public string? Description { get; set; }
            public List<FieldJson>? Fields { get; set; }
        }
    }
}
