// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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

        public override string ToString() => Name ?? "(Unnamed)";

        internal Container ToContainer(ModuleJson module, FieldPath path, ModuleAddress address, string description, FieldCondition? condition)
        {
            // TODO: Check that all fields are either primitive or container, check the size etc.
            List<Fields.IField> fields = Fields
                .SelectMany(fieldJson => fieldJson.ToFields(module, path, address))
                .ToList();
            var lastField = Fields.LastOrDefault();
            int size = Size?.Value ?? lastField?.Offset?.Value ?? 0;

            return new Container(path, address, size, description, condition, Name, fields.AsReadOnly());
        }
    }
}
