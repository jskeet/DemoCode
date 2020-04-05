// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics.CodeAnalysis;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// Common preconditions.
    /// </summary>
    public static class Preconditions
    {
        /// <summary>
        /// Validates that a value which is declared to be not null is actually not null.
        /// This method is used to check member arguments, where the member signature says
        /// that null values are not permitted.
        /// </summary>
        /// <typeparam name="T">The type of value to check.</typeparam>
        /// <param name="value">The value to check for nullity.</param>
        /// <param name="paramName">The name of the parameter the value came from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="paramName"/> is null.</exception>
        /// <returns>The same value, if it's not null.</returns>
        public static T CheckNotNull<T>(T value, string paramName)
            where T : class =>
            value ?? throw new ArgumentNullException(paramName);

        /// <summary>
        /// Validates that a value which is nullable according to the type system is really
        /// not null, due to previous validation. The annotation ensures that after this call
        /// (within the same piece of code) the compiler is aware that the value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <exception cref="InvalidOperationException"><paramref name="value"/> is null.</exception>
        /// <returns>The same value, if it's not null.</returns>
        public static T AssertNotNull<T>([NotNull] T? value) where T : class
            => value ?? throw new InvalidOperationException();
    }
}
