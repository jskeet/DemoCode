// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Json
{
    /// <summary>
    /// The JSON representation of information about a dynamic overlay.
    /// </summary>
    internal sealed class DynamicOverlayJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// The size of the overlay, in bytes.
        /// Each container is expected to have a size of this value plus the offset of the parent field,
        /// and every offset should be greater than or equal to the offset of the parent field.
        /// </summary>
        public HexInt32? Size { get; set; }

        /// <summary>
        /// The offset of the container holding the switch field, relative to the start of the parent container
        /// of this field.
        /// </summary>
        public HexInt32? SwitchContainerOffset { get; set; }

        /// <summary>
        /// The name of the switch field within the switch container.
        /// </summary>
        public string? SwitchField { get; set; }

        /// <summary>
        /// The containers to switch between.
        /// </summary>
        public List<ContainerJson>? Containers { get; set; }
    }
}
