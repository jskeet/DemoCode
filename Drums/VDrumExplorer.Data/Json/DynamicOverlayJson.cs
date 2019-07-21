using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public string Comment { get; set; }

        /// <summary>
        /// The size of the overlay, in bytes.
        /// Each container is expected to have a size of this value plus the offset of the parent field,
        /// and every offset should be greater than or equal to the offset of the parent field.
        /// </summary>
        public HexString Size { get; set; }

        /// <summary>
        /// The offset relative to the start of the parent container, at which
        /// to find the value to switch between dynamic overlays. This may be an offset into a different
        /// container.
        /// </summary>
        public HexString SwitchOffset { get; set; }

        /// <summary>
        /// Any transform to apply. Currently supported value "instrumentGroup", which expects
        /// the target of the offset to be an instrument number, then converted into an instrument group.
        /// </summary>
        public string SwitchTransform { get; set; }

        /// <summary>
        /// The containers to switch between.
        /// </summary>
        public List<ContainerJson> Containers { get; set; }
    }
}
