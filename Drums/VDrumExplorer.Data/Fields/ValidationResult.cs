// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class ValidationResult
    {
        public int TotalFields { get; }
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationResult(int totalFields, IReadOnlyList<ValidationError> errors) =>
            (TotalFields, Errors) = (totalFields, errors);
    }
}
