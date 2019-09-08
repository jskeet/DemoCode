﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.IO;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Proto;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// A module consists of a <see cref="ModuleData"/> and
    /// the <see cref="ModuleSchema"/> that the data abides by.
    /// </summary>
    public sealed class Module
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }

        public Module(ModuleSchema schema, ModuleData data) =>
            (Schema, Data) = (schema, data);

        /// <summary>
        /// Loads module data, autodetecting the schema using the <see cref="SchemaRegistry"/>.
        /// </summary>
        public static Module FromStream(Stream stream) => ProtoIo.ReadModule(stream);

        /// <summary>
        /// Validates that every field in the schema has a valid value.
        /// This will fail if only partial data has been loaded.
        /// </summary>
        public ValidationResult Validate()
        {
            int count = 0;
            var errors = new List<ValidationError>();
            foreach (var annotatedContainer in Schema.GetContainers())
            {
                var context = annotatedContainer.Context;
                foreach (var field in context.GetPrimitiveFields(Data))
                {
                    count++;
                    if (!field.Validate(context, Data, out var message))
                    {
                        // TODO: use NotNullWhen to avoid needing the dammit operator
                        errors.Add(new ValidationError(annotatedContainer.AnnotateChildField(field), message!));
                    }
                }
            }
            return new ValidationResult(count, errors.AsReadOnly());
        }

        public void Save(Stream stream) => ProtoIo.Write(stream, this);        
    }
}
