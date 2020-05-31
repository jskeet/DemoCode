// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// Extension methods that are in .NET Standard 2.1, but which we can't use yet...
    /// </summary>
    public static class CompatibilityExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
            GetValueOrDefault(dictionary, key, default!);

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) =>
            dictionary.TryGetValue(key, out var value) ? value : defaultValue;

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value) =>
            (key, value) = (pair.Key, pair.Value);

        public static unsafe string GetString(this Encoding encoding, Span<byte> bytes)
        {
            fixed (byte* ptr = bytes)
            {
                return encoding.GetString(ptr, bytes.Length);
            }
        }

        public static unsafe void GetBytes(this Encoding encoding, ReadOnlySpan<char> text, Span<byte> bytes)
        {
            fixed (byte* outputPtr = bytes)
            {
                fixed (char* inputPtr = text)
                {
                    encoding.GetBytes(inputPtr, text.Length, outputPtr, bytes.Length);
                }
            }
        }
    }
}
