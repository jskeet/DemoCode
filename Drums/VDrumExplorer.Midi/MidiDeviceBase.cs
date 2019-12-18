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
        internal string SystemDeviceId { get; }

        /// <summary>
        /// Human-readable device name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// MIDI manufacturer.
        /// </summary>
        internal string Manufacturer { get; }

        protected MidiDeviceBase(string systemDeviceId, string name, string manufacturer) =>
            (SystemDeviceId, Name, Manufacturer) = (systemDeviceId, name, manufacturer);

        public override string ToString() => $"{SystemDeviceId}: {Name} ({Manufacturer})";
    }

    /// <summary>
    /// MIDI input device.
    /// </summary>
    public sealed class MidiInputDevice : MidiDeviceBase
    {
        internal MidiInputDevice(string systemDeviceId, string name, string manufacturer)
            : base(systemDeviceId, name, manufacturer)
        {
        }
    }

    /// <summary>
    /// MIDI output device.
    /// </summary>
    public sealed class MidiOutputDevice : MidiDeviceBase
    {
        internal MidiOutputDevice(string systemDeviceId, string name, string manufacturer)
            : base(systemDeviceId, name, manufacturer)
        {
        }
    }
}
