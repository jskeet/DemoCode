using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models.Fields
{
    /// <summary>
    /// A data container representing a portion of memory.
    /// </summary>
    public sealed class Container : FieldBase, IContainerField
    {
        public IReadOnlyList<IField> Fields { get; }
        public bool Loadable { get; }
        
        // FIXME: Do we want this? 
        public string Name { get; }

        internal Container(string name, string description, string path, ModuleAddress address, int size, IReadOnlyList<IField> fields)
            : base(description, path, address, size)
        {
            Name = name;
            Fields = fields;
            Loadable = !Fields.Any(f => f is Container);
        }

        public IEnumerable<IField> GetFields(ModuleData data) => Fields;
    }
}
