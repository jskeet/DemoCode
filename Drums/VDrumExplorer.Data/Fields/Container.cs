// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A data container representing a portion of memory.
    /// </summary>
    public sealed class Container : FieldBase
    {
        public IReadOnlyList<IField> Fields { get; }
        private readonly IDictionary<string, IField> fieldsByName;
        public bool Loadable { get; }
        public new int Size { get; }

        internal Container(FieldBase.Parameters common, IReadOnlyList<IField> fields)
            : base(common)
        {
            Size = base.Size;
            Fields = fields;
            Loadable = !Fields.Any(f => f is Container);
            fieldsByName = fields.ToDictionary(f => f.Name);
        }

        internal IField GetField(string name) => fieldsByName[name];

        /// <summary>
        /// Resets all primitive fields in the container to valid values, recursively.
        /// (For the moment, we'll assume that's all that's required.)
        /// </summary>
        public void Reset(FixedContainer context, ModuleData data)
        {
            foreach (var field in Fields.OfType<IPrimitiveField>())
            {
                field.Reset(context, data);
            }
        }
    }
}
