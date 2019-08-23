// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Json
{
    /// <summary>
    /// The JSON representation of information about the dynamic overlay 
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
        /// The offset relative to the start of the parent container, at which
        /// to find the value to switch between dynamic overlays. This may be an offset into a different
        /// container.
        /// </summary>
        public HexInt32? SwitchOffset { get; set; }

        /// <summary>
        /// Any transform to apply. Currently supported value "instrumentGroup", which expects
        /// the target of the offset to be an instrument field. The instrument group is determined from that.
        /// (An extra container is expected after all the preset instruments, for user samples, as they're
        /// not in an instrument group.)
        /// </summary>
        public string? SwitchTransform { get; set; }

        /// <summary>
        /// The containers to switch between.
        /// </summary>
        public List<ContainerJson>? Containers { get; set; }
    }
}
