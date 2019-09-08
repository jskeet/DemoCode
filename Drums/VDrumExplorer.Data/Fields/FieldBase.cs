// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    public abstract class FieldBase : IField
    {
        public ModuleSchema Schema { get; }
        public string Description { get; }
        public int Size { get; }
        public FieldCondition? Condition { get; }

        public string Name { get; }
        public int Offset { get; }

        private protected FieldBase(Parameters p) =>
            (Schema, Name, Offset, Size, Description, Condition) =
            (p.Schema, p.Name, p.Offset, p.Size, p.Description, p.Condition);

        public override string ToString() => Description;

        protected ModuleAddress GetAddress(FixedContainer context) => context.Address + Offset;

        /// <summary>
        /// Common parameters for FieldBase, extracted into a class to make it much simpler to add/remove items.
        /// </summary>
        internal class Parameters
        {
            public ModuleSchema Schema { get; }
            public string Description { get; }
            public string Name { get; }
            public int Offset { get; }
            public int Size { get; }
            public FieldCondition? Condition { get; }

            public Parameters(ModuleSchema schema, string name, int offset, int size, string description, FieldCondition? condition) =>
                (Schema, Name, Offset, Size, Description, Condition) = (schema, name, offset, size, description, condition);
        }
    }
}
