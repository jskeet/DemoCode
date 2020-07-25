﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Physical
{
    /// <summary>
    /// A container that only contains fields.
    /// </summary>
    public sealed class FieldContainer : ContainerBase
    {
        public static IComparer<FieldContainer> AddressComparer { get; } = new AddressComparerImpl();

        /// <summary>
        /// The fields within this container.
        /// </summary>
        public IReadOnlyList<IField> Fields { get; }

        /// <summary>
        /// A map from field name to field.
        /// </summary>
        private IReadOnlyDictionary<string, IField> FieldsByName => fieldsByName.Value;

        private readonly Lazy<IReadOnlyDictionary<string, IField>> fieldsByName;

        /// <summary>
        /// The size of this container, in bytes.
        /// </summary>
        public int Size { get; }

        internal FieldContainer(ModuleSchema schema, string name, string description, ModuleAddress address, string path,
            int size, IEnumerable<FieldBase> fields)
            : base(schema, name, description, address, path)
        {
            Fields = fields.ToReadOnlyList(field => field.WithParent(this));
            Size = size;
            fieldsByName = Lazy.Create(() => Fields.ToDictionary(f => f.Name).AsReadOnly());
        }

        public class AddressComparerImpl : IComparer<FieldContainer>
        {
            public int Compare(FieldContainer x, FieldContainer y) =>
                x.Address.CompareTo(y.Address);
        }

        internal IField? GetFieldOrNull(ReadOnlySpan<char> fieldName) => FieldsByName.GetValueOrDefault(fieldName.ToString());
    }
}
