// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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
        public int Size { get; }

        /// <summary>
        /// The name of the field, which is unique within its container.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The description of the field.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The offset of the field within its container.
        /// </summary>
        public ModuleOffset Offset { get; }

        private protected FieldBase(Parameters p) =>
            (Name, Description, Offset, Size) =
            (p.Name, p.Description, p.Offset, p.Size);

        public override string ToString() => Description;

        /// <summary>
        /// Common parameters for FieldBase, extracted into a class to make it much simpler to add/remove items.
        /// </summary>
        internal class Parameters
        {
            public string Name { get; }
            public string Description { get; }
            public ModuleOffset Offset { get; }
            public int Size { get; }

            public Parameters(string name, string description, ModuleOffset offset, int size) =>
                (Name, Description, Offset, Size) = (name, description, offset, size);
        }
    }
}
