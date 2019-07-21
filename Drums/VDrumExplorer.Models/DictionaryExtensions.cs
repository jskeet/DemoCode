using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDrumExplorer.Models
{
    internal static class DictionaryExtensions
    {
        internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
            new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}
