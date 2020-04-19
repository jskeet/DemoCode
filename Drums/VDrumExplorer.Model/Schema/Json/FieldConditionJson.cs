// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Schema.Json
{
    internal sealed class FieldConditionJson
    {
        /// <summary>
        /// The name of the condition field (within the same container).
        /// </summary>
        public string? Field { get; set; }
        
        /// <summary>
        /// The required value of the condition field.
        /// </summary>
        public int? RequiredValue { get; set; }

        internal void Validate(string fieldName)
        {
            Validation.Validate(Field is object, $"{nameof(Field)} must be specified in condition for field {fieldName}");
            Validation.Validate(RequiredValue.HasValue, $"{nameof(RequiredValue)} must be specified in condition for field {fieldName}");
        }

        internal Condition ToCondition() => new Condition(Field!, RequiredValue!.Value);
    }
}
