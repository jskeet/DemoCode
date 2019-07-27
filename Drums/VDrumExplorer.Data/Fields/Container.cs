using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A data container representing a portion of memory.
    /// </summary>
    public sealed class Container : FieldBase, IContainerField
    {
        public IReadOnlyList<IField> Fields { get; }
        public bool Loadable { get; }
        
        // FIXME: Do we want this? 
        public string? Name { get; }

        internal Container(FieldPath path, ModuleAddress address, int size, string description, string? name, IReadOnlyList<IField> fields)
            : base(path, address, size, description)
        {
            Name = name;
            Fields = fields;
            Loadable = !Fields.Any(f => f is Container);
        }

        public IEnumerable<IField> Children(ModuleData data) => Fields;

        public IEnumerable<IField> DescendantsAndSelf()
        {
            yield return this;
            foreach (var field in Fields)
            {
                if (field is Container container)
                {
                    foreach (var descendant in container.DescendantsAndSelf())
                    {
                        yield return descendant;
                    }
                }
                else
                {
                    yield return field;
                }
            }
        }
    }
}
