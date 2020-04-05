﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf.WellKnownTypes;
using System.Linq;

namespace VDrumExplorer.Proto
{
    internal partial class ModuleAudio
    {
        internal Model.Audio.ModuleAudio ToModel()
        {
            var schema = Identifier.GetSchema();
            var format = Format.ToModel();
            var duration = DurationPerInstrument.ToTimeSpan();
            var captures = InstrumentCaptures.Select(ic => ic.ToModel(schema)).ToList().AsReadOnly();
            return new Model.Audio.ModuleAudio(schema, format, duration, captures);
        }

        internal static ModuleAudio FromModel(Model.Audio.ModuleAudio audio) =>
            new ModuleAudio
            {
                Identifier = ModuleIdentifier.FromModel(audio.Schema.Identifier),
                Format = AudioFormat.FromModel(audio.Format),
                DurationPerInstrument = Duration.FromTimeSpan(audio.DurationPerInstrument),
                InstrumentCaptures = { audio.Captures.Select(InstrumentAudio.FromModel) }
            };
    }
}
