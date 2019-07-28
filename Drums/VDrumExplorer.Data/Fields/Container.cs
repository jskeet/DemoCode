using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A data container representing a portion of memory.
    /// </summary>
    public sealed class Container : FieldBase, IContainerField
    {
        public IReadOnlyList<IField> Fields { get; }
        internal IReadOnlyDictionary<ModuleAddress, IField> FieldsByAddress { get; }
        public bool Loadable { get; }
        
        public string? Name { get; }

        internal Container(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition, string? name, IReadOnlyList<IField> fields)
            : base(path, address, size, description, condition)
        {
            Name = name;
            Fields = fields;
            Loadable = !Fields.Any(f => f is Container);
            FieldsByAddress = Fields.ToDictionary(f => f.Address).AsReadOnly();
        }

        public IEnumerable<IField> Children(ModuleData data) => Fields;

        /// <summary>
        /// Returns all fields in this container recursively. Dynamic overlay fields are returned as they are,
        /// with no further resolution.
        /// </summary>
        /// <returns>A sequence of fields, including the container itself.</returns>
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

        /// <summary>
        /// Returns all fields in this container recursively. Dynamic overlay fields are processed
        /// according to the data in <paramref name="data"/>; the field itself is not returned,
        /// but the overlaid fields are, having resolved the relevant container.
        /// </summary>
        /// <returns>A sequence of fields, including the container itself.</returns>
        public IEnumerable<IField> DescendantsAndSelf(ModuleData data)
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
                else if (field is DynamicOverlay overlay)
                {
                    foreach (var descendant in overlay.Children(data))
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
