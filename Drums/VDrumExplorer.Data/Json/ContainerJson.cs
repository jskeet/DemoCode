using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    internal class ContainerJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Name of the container.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Must be absent for all containers which reference other containers.
        /// Must be present for all containers with just primitive fields.
        /// </summary>
        public HexInt32? Size { get; set; }
        public List<FieldJson>? Fields { get; set; }

        public void Validate()
        {
            // TODO: Check that all fields are either primitive or container, check the size etc.
        }

        public override string ToString() => Name ?? "(Unnamed)";

        internal Container ToContainer(ModuleJson module, FieldPath path, ModuleAddress address, string description)
        {
            List<Fields.IField> fields = Fields
                .SelectMany(fieldJson => fieldJson.ToFields(module, path, address))
                .ToList();
            int size = Size?.Value ?? ((fields.Last().Address + fields.Last().Size) - address);

            return new Container(path, address, size, description, Name, fields.AsReadOnly());
        }
    }
}
