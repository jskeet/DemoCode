// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NdiStreamDeck
{
    /// <summary>
    /// Workarounds for minor bugs in the (unofficial) C# Stream Deck SDK.
    /// </summary>
    internal static class StreamDeckWorkarounds
    {
        // Workaround for https://github.com/OpenMacroBoard/StreamDeckSharp/issues/44
        internal static void ClearKeysWithWorkaround(this IStreamDeckBoard deck)
        {
            // Note: while you might expect KeyBitmap.Create.FromRgb(0, 0, 0) would work,
            // the bug scuppers that too.
            var width = deck.Keys.KeyWidth;
            var height = deck.Keys.KeyHeight;
            var emptyBitmap = new KeyBitmap(width, height, new byte[width * height * 3]);
            for (int i = 0; i < deck.Keys.Count; i++)
            {
                deck.SetKeyBitmap(i, emptyBitmap);
            }
        }

        // Workarounds for https://github.com/OpenMacroBoard/OpenMacroBoard.SDK/issues/18
        // Both assume we're not dealing with a grid that only has a single row or a single column.
        internal static int GetKeyDistanceXWorkaround(this GridKeyPositionCollection keys) =>
            keys[1].X - keys.KeyWidth;

        internal static int GetKeyDistanceYWorkaround(this GridKeyPositionCollection keys) =>
            keys[keys.KeyCountX].Y - keys.KeyHeight;
    }
}
