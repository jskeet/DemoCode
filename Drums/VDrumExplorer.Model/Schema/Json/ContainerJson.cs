// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#nullable disable warnings

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Json
{
    /// <summary>
    /// The JSON representation of a container. Containers can either be
    /// "container containers" or "field containers". "Container containers"
    /// can only contain other containers, and have no overlays. "Field containers"
    /// can only contain fields and an optional overlay.
    /// 
    /// The process of generating the schema model is:
    /// 
    /// - Process the ModuleJson so we know the lookups, counts, instruments etc.
    /// - Load all the containers as ContainerJson objects
    /// - Call ValidateAndResolve on all ContainerJson objects
    ///   - This will resolve names and descriptions, and generate fields and overlays.
    /// - Call ToContainer on the root ContainerJson, which will recursively populate the tree.
    /// 
    /// The overall result is a complete tree of containers, each with a separate address and path.
    /// Each field is only resolved a single time within each container (although possibly still
    /// generating multiple fields due to repeated fields, but *not* repeated containers). Each field
    /// has a well-defined offset within its parent container, but full address and path are not known
    /// as the same field can appear within multiple parent containers.
    /// </summary>
    internal class ContainerJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Must be present for all field containers.
        /// </summary>
        public HexInt32? Size { get; set; }

        /// <summary>
        /// Must be present for all field containers, and
        /// absent for all container containers.
        /// </summary>
        public List<FieldJson>? Fields { get; set; }

        /// <summary>
        /// Must be present for all container containers,
        /// and absent for all field containers.
        /// </summary>
        public List<ContainerReferenceJson>? Containers { get; set; }

        /// <summary>
        /// Field containers have their fields resolved once, then reused across
        /// multiple parent containers.
        /// </summary>
        private IReadOnlyList<IField>? resolvedFields;

        /// <summary>
        /// If none of the fields are overlays, we can just use resolvedFields directly.
        /// Otherwise, we need to create a new list of fields for each container, because
        /// overlays need variable substitution.
        /// </summary>
        private bool requiresOverlayResolution;

        /// <summary>
        /// The name of the container, specified in the top-level module JSON. This is
        /// populated by code rather than by JSON deserialization.
        /// </summary>
        public string? NameInModuleDictionary { get; set; }

        internal void ValidateAndResolve(ModuleJson module)
        {
            Validation.Validate(Fields is object ^ Containers is object,
                "Each container must either specify fields or containers as children, but not both.");

            if (Fields is object)
            {
                Validation.Validate(Size is object, "Field containers must specify a size.");
                Fields.ForEach(f => f.ValidateJson(module));
                resolvedFields = ResolveFields(module);
                requiresOverlayResolution = resolvedFields.Any(f => f is OverlayField overlay && overlay.SwitchPath.Contains('{'));
            }
            else
            {
                Containers?.ForEach(f => f.ValidateJson(module));
            }
        }

        private IReadOnlyList<IField> ResolveFields(ModuleJson module)
        {
            var resolved = new List<IField>();
            foreach (var field in Fields)
            {
                var lastField = resolved.LastOrDefault();
                var offset = lastField?.Offset + lastField?.Size ?? ModuleOffset.Zero;
                resolved.AddRange(field.ToFields(module, offset));
            }
            // Skip placeholder fields so they're never exposed.
            return resolved.Where(field => !(field is PlaceholderField)).ToReadOnlyList();
        }

        public IContainer ToContainer(ModuleSchema schema, ModuleJson module, string name, string description, ModuleAddress address, string? parentPath, SchemaVariables variables)
        {
            string path = PathUtilities.AppendPath(parentPath, name);
            if (Fields is object)
            {
                var fieldList = requiresOverlayResolution ? resolvedFields.Select(FinalizeField).ToList().AsReadOnly() : resolvedFields;
                return new FieldContainer(schema, name, description, address, path, Size.Value, fieldList);

                IField FinalizeField(IField field) =>
                    field is OverlayField overlay ? overlay.WithPath(variables.Replace(overlay.SwitchPath)) : field;
            }
            else
            {
                var containers = new List<IContainer>();
                foreach (var container in Containers)
                {
                    containers.AddRange(container.ToContainers(schema, module, address, path, variables));
                }
                return new ContainerContainer(schema, name, description, address, path, containers);
            }
        }
    }
}
