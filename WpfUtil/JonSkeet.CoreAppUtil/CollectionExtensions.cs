// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace JonSkeet.CoreAppUtil;

/// <summary>
/// Convenient extension methods for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Creates a read-only copy of the given sequence, as a list.
    /// </summary>
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source) =>
        source.ToList().AsReadOnly();

    /// <summary>
    /// Creates a read-only copy of the given sequence, as a list, first applying a projection.
    /// </summary>
    public static IReadOnlyList<TResult> ToReadOnlyList<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) =>
        source.Select(selector).ToList().AsReadOnly();
}
