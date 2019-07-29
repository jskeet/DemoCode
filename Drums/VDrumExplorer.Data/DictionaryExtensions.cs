// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDrumExplorer.Data
{
    internal static class DictionaryExtensions
    {
        internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
            new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}
