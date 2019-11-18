// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// Base class for MIDI devices. The subclasses only exist for type safety purposes.
    /// </summary>
    public abstract class MidiDeviceBase
    {
        /// <summary>
        /// Local device ID, used to create an InputDevice/OutputDevice.
        /// </summary>
        internal int SystemDeviceId { get; }

        /// <summary>
        /// Human-readable device name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Midi manufacturer ID; this appears to always return 1, so is not reliable.
        /// </summary>
        internal ManufacturerId ManufacturerId { get; }

        /// <summary>
        /// Midi product ID.
        /// </summary>
        internal short ProductId { get; }

        protected MidiDeviceBase(int systemDeviceId, string name, ManufacturerId manufacturerId, short productId) =>
            (SystemDeviceId, Name, ManufacturerId, ProductId) = (systemDeviceId, name, manufacturerId, productId);

        public override string ToString() => $"{SystemDeviceId}: {Name} ({ManufacturerId} - {ProductId})";
    }

    /// <summary>
    /// MIDI input device.
    /// </summary>
    public sealed class MidiInputDevice : MidiDeviceBase
    {
        internal MidiInputDevice(int localDeviceId, string name, ManufacturerId manufacturerId, short productId)
            : base(localDeviceId, name, manufacturerId, productId)
        {
        }
    }

    /// <summary>
    /// MIDI output device.
    /// </summary>
    public sealed class MidiOutputDevice : MidiDeviceBase
    {
        internal MidiOutputDevice(int localDeviceId, string name, ManufacturerId manufacturerId, short productId)
            : base(localDeviceId, name, manufacturerId, productId)
        {
        }
    }
}
