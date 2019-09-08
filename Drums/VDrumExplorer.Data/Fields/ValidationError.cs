// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public sealed class ValidationError
    {
        public AnnotatedField Field { get; }
        public string Message { get; }
        
        public ValidationError(AnnotatedField field, string message) =>
            (Field, Message) = (field, message);
    }
}
