// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    using static Validation;
    
    /// <summary>
    /// A simple lookup in a schema, with no data dependencies.
    /// </summary>
    public sealed class LookupJson
    {
        public string? Name { get; set; }
        /// <summary>
        /// The variable name in "counts" that this lookup should correspond to.
        /// </summary>
        public string? Size { get; set; }
        public List<string>? Values { get; set; }        
        
        public void Validate(Func<string?, int?> getCount)
        {
            ValidateNotNull(Name, nameof(Name));
            ValidateNotNull(Size, nameof(Size));
            ValidateNotNull(Values, nameof(Values));
            Validation.Validate(Values.All(v => v != null), "Lookup values cannot contain null");
            var expectedSize = getCount(Size);
            Validation.Validate(expectedSize != null, $"Counts does not contain {Size} used in lookup {Name}");
            Validation.Validate(Values!.Count == expectedSize, $"Expected {expectedSize} values; actually {Values.Count}");
        }
    }
}
