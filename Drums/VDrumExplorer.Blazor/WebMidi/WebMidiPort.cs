// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text.Json.Serialization;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    /// <summary>
    /// MIDI port representation used for interop.
    /// </summary>
    internal class WebMidiPort
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        internal MidiInputDevice ToMidiInputDevice() => new MidiInputDevice(Id, Name, Manufacturer);
        internal MidiOutputDevice ToMidiOutputDevice() => new MidiOutputDevice(Id, Name, Manufacturer);
    }
}
