// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using System;
using System.Linq;

namespace DmxLighting
{
    // TODO: Consider using spans, if AYS will be okay with that.
    // (Need to check target versions.)

    /// <summary>
    /// A DMX universe of up to 512 channels, each with a byte value
    /// (0-255).
    /// </summary>
    public sealed class DmxUniverse
    {
        /// <summary>
        /// The maximum size of a DMX universe. Smaller universes
        /// can be created when fewer channels are needed, leading
        /// to smaller network packets being transmitted.
        /// </summary>
        public const int MaxSize = 512;

        /// <summary>
        /// The values for the channels. The first byte will always
        /// be zero, to allow simple 1-based indexing.
        /// </summary>
        private readonly byte[] channels;

        /// <summary>
        /// Raised when the data in the channels has changed.
        /// Event handlers should not modify the byte array as it is
        /// shared between all handlers, but it does not represent the
        /// "raw" underlying array.
        /// </summary>
        public EventHandler<byte[]> ChannelsChanged;

        /// <summary>
        /// The number of the universe.
        /// </summary>
        public short UniverseNumber { get; }

        /// <summary>
        /// Retrieves or sets the value of a single channel.
        /// </summary>
        /// <param name="channel">The 1-based channel to retrieve.</param>
        /// <returns>The value of the channel.</returns>
        public byte this[int channel]
        {
            get => channels[channel];
            // TODO: Make this more efficient!
            set => SetChannelValues(channel, new[] { value });
        }

        /// <summary>
        /// Creates a universe with a universe number of 1, and the maximum
        /// size.
        /// </summary>
        public DmxUniverse() : this(1, MaxSize)
        {
        }

        /// <summary>
        /// Creates a universe with the given size.
        /// </summary>
        /// <param name="size">The size of the universe, as the number of channels it supports.</param>
        public DmxUniverse(short universeNumber, int size)
        {
            UniverseNumber = universeNumber; // TODO: validate
            if (size < 1 || size > MaxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            // One larger to make 1-based indexing simpler.
            channels = new byte[size + 1];
        }

        /// <summary>
        /// Returns a clone of the current channel data.
        /// </summary>
        /// <returns>The values of the channels. The 0th element of the array is unused;
        /// channel 1 is stored in element 1 etc.</returns>
        public byte[] CloneChannels() => channels.ToArray();

        /// <summary>
        /// Sets one or more channel values.
        /// </summary>
        /// <param name="startChannel">The first channel to modify; 1-based.</param>
        /// <param name="values">The new values from <paramref name="startChannel"/> onwards.</param>
        public void SetChannelValues(int startChannel, byte[] values)
        {
            Buffer.BlockCopy(values, 0, channels, startChannel, values.Length);
            ChannelsChanged?.Invoke(this, channels.ToArray());
        }

        /// <summary>
        /// Modifiers channel values atomically (with respect to <see cref="ChannelsChanged"/>).
        /// </summary>
        /// <param name="modifier"></param>
        public void SetChannelValues(Action<byte[]> modifier)
        {
            var clone = channels.ToArray();
            modifier(clone);
            Buffer.BlockCopy(clone, 0, channels, 0, channels.Length);
            // We don't need to clone again, unless something's really 
            ChannelsChanged?.Invoke(this, clone);
        }
    }
}
