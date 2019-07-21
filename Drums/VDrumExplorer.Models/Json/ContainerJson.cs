using System.Collections.Generic;

namespace VDrumExplorer.Models.Json
{
    internal class ContainerJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Name of the container.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Must be absent for all containers which reference other containers.
        /// Must be present for all containers with just primitive fields.
        /// </summary>
        public HexString Size { get; set; }
        public List<FieldJson> Fields { get; set; }

        public void Validate()
        {
            // TODO: Check that all fields are either primitive or container, check the size etc.
        }

        public override string ToString() => Name;
    }
}
