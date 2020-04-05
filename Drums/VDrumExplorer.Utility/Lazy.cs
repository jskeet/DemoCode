// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Utility
{
    // Note: If we need overloads accepting isThreadSafe and threadSafetyMode, they're easy enough to add.

    /// <summary>
    /// Non-generic class with factory methods for <see cref="Lazy{T}"/> to
    /// make initialization briefer via generic type inference.
    /// </summary>
    public static class Lazy
    {
        /// <summary>
        /// Creates a <see cref="Lazy{T}"/> using the given factory.
        /// </summary>
        /// <typeparam name="T">The type of the result of the factory.</typeparam>
        /// <param name="valueFactory">The factory to use when creating the value.</param>
        /// <returns>A <see cref="Lazy{T}"/> using the given factory.</returns>
        public static Lazy<T> Create<T>(Func<T> valueFactory) => new Lazy<T>(valueFactory);


        // The Initialize methods help with variance: the valueFactory can provide a derived type,
        // but we know the actual type from field.
        /// <summary>
        /// Initializes a field to be a <see cref="Lazy{T}"/> with the given value factory.
        /// While this is logically just an assignment to the field, generic type inference
        /// can work better when it knows the field type, as the factory can be a lambda expression
        /// whose "natural" result would be a subtype of the field's type.
        /// </summary>
        /// <typeparam name="T">The type of the result of the factory.</typeparam>
        /// <param name="field">The field to initialize.</param>
        /// <param name="valueFactory">The factory to use when creating the value.</param>
        /// <returns>A <see cref="Lazy{T}"/> using the given factory.</returns>
        public static void Initialize<T>(out Lazy<T> field, Func<T> valueFactory) =>
            field = new Lazy<T>(valueFactory);
    }
}
