// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A condition on a field, such that it is only effective when another field
    /// has a particular value.
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// The name of the field (within the same container) to obtain a value from.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// The value that the field named by <see cref="Field"/> must have in order
        /// for the field "owning" this condition to be effective.
        /// </summary>
        public int RequiredValue { get; }

        public Condition(string field, int requiredValue) =>
            (Field, RequiredValue) = (field, requiredValue);
    }
}
