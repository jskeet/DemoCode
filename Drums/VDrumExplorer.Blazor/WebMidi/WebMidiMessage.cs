// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text.Json.Serialization;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    /// <summary>
    /// MIDI message representation used for JS interop.
    /// </summary>
    internal class WebMidiMessage
    {
        [JsonPropertyName("data")]
        public byte[] Data { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }

        public static WebMidiMessage FromMidiMessage(MidiMessage message) =>
            new WebMidiMessage { Data = message.Data, Timestamp = (double) message.Timestamp };

        public MidiMessage ToMidiMessage() =>
            new MidiMessage(Data, (long) Timestamp);
    }
}
