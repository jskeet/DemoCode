// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;

namespace VDrumExplorer.Data.Proto
{
    internal partial class InstrumentAudio
    {
        internal Audio.InstrumentAudio ToModel(ModuleSchema schema)
        {
            var bank = Preset ? schema.PresetInstruments : schema.UserSampleInstruments;
            var instrument = bank[InstrumentId];
            return new Audio.InstrumentAudio(instrument, AudioData.ToByteArray());
        }

        internal static InstrumentAudio FromModel(Audio.InstrumentAudio audio) =>
            new InstrumentAudio
            {
                AudioData = ByteString.CopyFrom(audio.Audio),
                InstrumentId = audio.Instrument.Id,
                Preset = audio.Instrument.Group != null
            };
    }
}
