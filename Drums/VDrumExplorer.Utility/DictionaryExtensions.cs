// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// Extension methods for dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns a read-only wrapper around the given dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <returns>A read-only wrapper around the dictionary.</returns>
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
            new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}
