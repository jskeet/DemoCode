// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Json
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
    }
}
