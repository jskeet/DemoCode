// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// LINQ-like methods for common operations within V-Drum Explorer. This is only not called
    /// Enumerable to avoid causing ambiguities for regular static method calls like <see cref="Enumerable.Range"/>
    /// etc.
    /// </summary>
    public static class VDrumEnumerable
    {
        /// <summary>
        /// Converts the sequence into a read-only wrapper around a <see cref="List{T}"/>.
        /// This method materializes the sequence.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="source">The sequence to convert.</param>
        /// <returns>A read-only wrapper around a <see cref="List{T}"/> containing the items in the sequence.</returns>
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source) =>
            source.ToList().AsReadOnly();

        /// <summary>
        /// Converts the sequence into a read-only wrapper around a <see cref="List{TResult}"/>
        /// by first applying a transformation to each element. This method materializes the sequence.
        /// </summary>
        /// <typeparam name="TSource">The element type of the input sequence.</typeparam>
        /// <typeparam name="TResult">The element type of the result list.</typeparam>
        /// <param name="source">The sequence to convert.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A read-only wrapper around a <see cref="List{T}"/> containing the items in the sequence.</returns>
        public static IReadOnlyList<TResult> ToReadOnlyList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            source.Select(selector).ToList().AsReadOnly();
    }
}
