// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model.Json
{
    internal sealed class FieldConditionJson
    {
        /// <summary>
        /// The address of the condition field, relative to the field's parent container.
        /// </summary>
        public HexInt32? Offset { get; set; }
        
        /// <summary>
        /// The required value of the condition field.
        /// </summary>
        public int? RequiredValue { get; set; }

        internal void Validate(string fieldName, ModuleJson module)
        {
            Validation.Validate(Offset is object, $"{nameof(Offset)} must be specified in condition for field {fieldName}");
            Validation.Validate(RequiredValue.HasValue, $"{nameof(RequiredValue)} must be specified in condition for field {fieldName}");
        }
    }
}
