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

    /// <summary>
    /// Returns the count of the given list, followed by the singular or plural form.
    /// </summary>
    public static string CountText<T>(this IReadOnlyList<T> list, string singular, string plural) =>
        $"{list.Count} {(list.Count == 1 ? singular : plural)}";

    /// <summary>
    /// Convenience for calling <see cref="CountText{T}(IReadOnlyList{T}, string, string)"/>
    /// when the plural is just "singular + 's'"
    /// </summary>
    public static string CountText<T>(this IReadOnlyList<T> list, string singular) =>
        $"{list.Count} {singular}{(list.Count == 1 ? "" : "s")}";
}
