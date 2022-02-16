// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NewTek.NDI;
using OpenMacroBoard.SDK;
using StreamDeckSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NdiStreamDeck
{
    /// <summary>
    /// A view of potentially multiple NDI sources on a single Stream Deck.
    /// </summary>
    internal sealed class NdiStreamDeckView : IDisposable
    {
        private readonly IStreamDeckBoard deck;
        private readonly IList<VideoFrameReceiver> sources;
        private int activeSourceIndex;

        // Just for convenience...
        private readonly int rows;
        private readonly int columns;
        private readonly int keyWidth;
        private readonly int keyHeight;
        private readonly int keyGapWidth;
        private readonly int keyGapHeight;

        // Note: not using IReadOnlyList as that doesn't have IndexOf :(
        internal NdiStreamDeckView(IStreamDeckBoard deck, IList<VideoFrameReceiver> sources)
        {
            this.deck = deck;
            rows = deck.Keys.KeyCountY;
            columns = deck.Keys.KeyCountX;
            keyWidth = deck.Keys.KeyWidth;
            keyHeight = deck.Keys.KeyHeight;

            keyGapWidth = deck.Keys.GetKeyDistanceXWorkaround();
            keyGapHeight = deck.Keys.GetKeyDistanceYWorkaround();

            this.sources = sources.Take(rows).ToList();
            deck.KeyStateChanged += HandleStreamDeckKey;
            foreach (var source in this.sources)
            {
                source.FrameReceived += DrawFrame;
            }
        }

        /// <summary>
        /// Draws the thumbnail for the given frame, and also the full picture
        /// if the source is the "active" one.
        /// </summary>
        private void DrawFrame(object sender, VideoFrame frame)
        {
            var source = (VideoFrameReceiver) sender;
            int index = sources.IndexOf(source);
            DrawThumbnail(index, frame);
            if (index == activeSourceIndex)
            {
                DrawFullFrame(frame);
            }
        }

        /// <summary>
        /// On key-down events in the first column, switch the active source.
        /// </summary>
        private void HandleStreamDeckKey(object sender, KeyEventArgs e)
        {
            if (!e.IsDown || e.Key % columns != 0)
            {
                return;
            }
            int row = e.Key / columns;
            if (row >= sources.Count)
            {
                return;
            }
            activeSourceIndex = row;
        }

        /// <summary>
        /// Draws a thumbnail view of the given frame on the first button
        /// of the given row.
        /// </summary>
        private unsafe void DrawThumbnail(int row, VideoFrame frame)
        {
            var height = keyHeight;
            var width = keyWidth;
            var data = new byte[width * height * 3];

            int scale = Math.Min(frame.Height / height, frame.Width / width);

            // Our simplistic scaling means we don't show absolutely everything,
            // so make sure we show the centre of the input image.
            int inputOffsetX = (frame.Width - width * scale) / 2;
            int inputOffsetY = (frame.Height - height * scale) / 2;

            int destIndex = 0;
            byte* buffer = (byte*) frame.BufferPtr.ToPointer();
            for (int y = 0; y < height; y++)
            {
                int sourceIndex = (y * scale + inputOffsetY) * frame.Stride + (inputOffsetX * 4);
                for (int x = 0; x < width; x++)
                {
                    data[destIndex++] = buffer[sourceIndex++];
                    data[destIndex++] = buffer[sourceIndex++];
                    data[destIndex++] = buffer[sourceIndex++];
                    sourceIndex++;
                    sourceIndex += (scale - 1) * 4;
                }
            }
            var bitmap = new KeyBitmap(width, height, data);
            deck.SetKeyBitmap(row * columns, bitmap);
        }

        /// <summary>
        /// Draws the given frame across all buttons except the first column.
        /// </summary>
        private unsafe void DrawFullFrame(VideoFrame frame)
        {
            // Assume we want the left-most column to be camera selection,
            // and all other buttons to be used for the "big display".
            int keyColumns = columns - 1;
            int keyRows = rows;
            int keyOffsetX = 1;
            int keyOffsetY = 0;

            int keyPixelsWidth = keyWidth * keyColumns + keyGapWidth * (keyColumns - 1);
            int keyPixelsHeight = keyHeight * keyRows + keyGapHeight * (keyRows - 1);
            int scale = Math.Min(frame.Height / keyPixelsHeight, frame.Width / keyPixelsWidth);

            // Our simplistic scaling means we don't show absolutely everything,
            // so make sure we show the centre of the input image.
            int inputOffsetX = (frame.Width - keyPixelsWidth * scale) / 2;
            int inputOffsetY = (frame.Height - keyPixelsHeight * scale) / 2;

            byte* buffer = (byte*) frame.BufferPtr.ToPointer();

            for (int row = 0; row < keyRows; row++)
            {
                int startY = row * (keyHeight + keyGapHeight) * scale + inputOffsetY;
                for (int col = 0; col < keyColumns; col++)
                {
                    int startX = col * (keyWidth + keyGapWidth) * scale + inputOffsetX;
                    var data = new byte[keyWidth * keyHeight * 3];
                    int destIndex = 0;
                    for (int y = 0; y < keyHeight; y++)
                    {
                        int sourceIndex = (y * scale + startY) * frame.Stride + startX * 4;
                        for (int x = 0; x < keyWidth; x++)
                        {
                            data[destIndex++] = buffer[sourceIndex++];
                            data[destIndex++] = buffer[sourceIndex++];
                            data[destIndex++] = buffer[sourceIndex++];
                            sourceIndex++;
                            sourceIndex += (scale - 1) * 4;
                        }
                    }
                    var bitmap = new KeyBitmap(keyWidth, keyHeight, data);
                    deck.SetKeyBitmap((row + keyOffsetY) * deck.Keys.KeyCountX + col + keyOffsetX, bitmap);
                }
            }
        }

        public void Dispose()
        {
            deck.ClearKeysWithWorkaround();
            deck.KeyStateChanged -= HandleStreamDeckKey;
            foreach (var source in sources)
            {
                source.FrameReceived -= DrawFrame;
            }
        }
    }
}
