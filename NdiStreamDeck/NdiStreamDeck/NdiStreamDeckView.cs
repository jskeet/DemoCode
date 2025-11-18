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
        private readonly IMacroBoard deck;
        private readonly IList<VideoFrameReceiver> sources;
        private int activeSourceIndex;

        // Just for convenience...
        private readonly int rows;
        private readonly int columns;
        private readonly int keySize;
        private readonly int gapSize;

        // Note: not using IReadOnlyList as that doesn't have IndexOf :(
        internal NdiStreamDeckView(IMacroBoard deck, IList<VideoFrameReceiver> sources)
        {
            this.deck = deck;
            rows = deck.Keys.CountY;
            columns = deck.Keys.CountX;
            keySize = deck.Keys.KeySize;
            gapSize = deck.Keys.GapSize;

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
            var height = keySize;
            var width = keySize;
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
            var bitmap = KeyBitmap.Create.FromBgr24Array(width, height, data);
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

            int keyPixelsWidth = keySize * keyColumns + gapSize * (keyColumns - 1);
            int keyPixelsHeight = keySize * keyRows + gapSize * (keyRows - 1);
            int scale = Math.Min(frame.Height / keyPixelsHeight, frame.Width / keyPixelsWidth);

            // Our simplistic scaling means we don't show absolutely everything,
            // so make sure we show the centre of the input image.
            int inputOffsetX = (frame.Width - keyPixelsWidth * scale) / 2;
            int inputOffsetY = (frame.Height - keyPixelsHeight * scale) / 2;

            byte* buffer = (byte*) frame.BufferPtr.ToPointer();

            for (int row = 0; row < keyRows; row++)
            {
                int startY = row * (keySize + gapSize) * scale + inputOffsetY;
                for (int col = 0; col < keyColumns; col++)
                {
                    int startX = col * (keySize + gapSize) * scale + inputOffsetX;
                    var data = new byte[keySize * keySize * 3];
                    int destIndex = 0;
                    for (int y = 0; y < keySize; y++)
                    {
                        int sourceIndex = (y * scale + startY) * frame.Stride + startX * 4;
                        for (int x = 0; x < keySize; x++)
                        {
                            data[destIndex++] = buffer[sourceIndex++];
                            data[destIndex++] = buffer[sourceIndex++];
                            data[destIndex++] = buffer[sourceIndex++];
                            sourceIndex++;
                            sourceIndex += (scale - 1) * 4;
                        }
                    }
                    var bitmap = KeyBitmap.Create.FromBgr24Array(keySize, keySize, data);
                    deck.SetKeyBitmap((row + keyOffsetY) * columns + col + keyOffsetX, bitmap);
                }
            }
        }

        public void Dispose()
        {
            deck.ClearKeys();
            deck.KeyStateChanged -= HandleStreamDeckKey;
            foreach (var source in sources)
            {
                source.FrameReceived -= DrawFrame;
            }
        }
    }
}
