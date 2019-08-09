// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public abstract class FieldBase : IField
    {
        public ModuleSchema Schema { get; }
        public string Description { get; }
        public FieldPath Path { get; }
        public ModuleAddress Address { get; }
        public int Size { get; }
        public FieldCondition? Condition { get; }

        private protected FieldBase(Parameters p) =>
            (Schema, Path, Address, Size, Description, Condition) =
            (p.Schema, p.Path, p.Address, p.Size, p.Description, p.Condition);

        public override string ToString() => Description;

        /// <summary>
        /// Common parameters for FieldBase, extracted into a class to make it much simpler to add/remove items.
        /// </summary>
        internal class Parameters
        {
            public ModuleSchema Schema { get; }
            public string Description { get; }
            public FieldPath Path { get; }
            public ModuleAddress Address { get; }
            public int Size { get; }
            public FieldCondition? Condition { get; }

            public Parameters(ModuleSchema schema, FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition) =>
                (Schema, Path, Address, Size, Description, Condition) = (schema, path, address, size, description, condition);
        }
    }
}
