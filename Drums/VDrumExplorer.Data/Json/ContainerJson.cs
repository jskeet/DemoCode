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
        /// Must be absent for all containers which reference other containers.
        /// Must be present for all containers with just primitive fields.
        /// </summary>
        public HexInt32? Size { get; set; }
        public List<FieldJson>? Fields { get; set; }

        internal Container ToContainer(ModuleSchema schema, ModuleJson module, string name, int offset, string description, FieldCondition? condition)
        {
            // TODO: Check that all fields are either primitive or container, check the size etc.
            List<Fields.IField> fields = Fields
                .SelectMany(fieldJson => fieldJson.ToFields(schema, module))
                .ToList();
            var lastField = Fields.LastOrDefault();
            int size = Size?.Value ?? lastField?.Offset?.Value ?? 0;
            var common = new FieldBase.Parameters(schema, name, offset, size, description, condition);
            return new Container(common, fields.AsReadOnly());
        }
    }
}
