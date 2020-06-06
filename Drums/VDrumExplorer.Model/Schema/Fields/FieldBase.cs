// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Base class for all fields.
    /// </summary>
    public abstract class FieldBase : IField
    {
        /// <summary>
        /// The size of the field, in bytes.
        /// </summary>
        public int Size => Parameters.Size;

        /// <summary>
        /// The name of the field, which is unique within its container.
        /// </summary>
        public string Name => Parameters.Name;

        /// <summary>
        /// The description of the field.
        /// </summary>
        public string Description => Parameters.Description;

        /// <summary>
        /// The offset of the field within its container.
        /// </summary>
        public ModuleOffset Offset => Parameters.Offset;

        private protected FieldParameters Parameters { get; }

        public FieldContainer? Parent { get; }

        private protected FieldBase(FieldContainer? parent, FieldParameters parameters) =>
            (Parent, Parameters) = (parent, parameters);

        public override string ToString() => Description;

        public string Path => Parent?.Path + "/" + Name;

        internal abstract FieldBase WithParent(FieldContainer parent);

        /// <summary>
        /// Common parameters for FieldBase, extracted into a class to make it much simpler to add/remove items.
        /// </summary>
        internal class FieldParameters
        {
            public string Name { get; }
            public string Description { get; }
            public ModuleOffset Offset { get; }
            public int Size { get; }

            public FieldParameters(string name, string description, ModuleOffset offset, int size) =>
                (Name, Description, Offset, Size) = (name, description, offset, size);
        }
    }
}
