// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System.Linq;
using VDrumExplorer.Model;

namespace VDrumExplorer.Proto
{
    internal partial class ModuleAudio
    {
        internal Model.Audio.ModuleAudio ToModel(ILogger logger)
        {
            var schema = Identifier.GetOrInferSchema(schema => InstrumentCaptures.All(ic => IsValidInstrument(schema, ic)), logger);
            var format = Format.ToModel();
            var duration = DurationPerInstrument.ToTimeSpan();
            var captures = InstrumentCaptures.Select(ic => ic.ToModel(schema)).ToList().AsReadOnly();
            return new Model.Audio.ModuleAudio(schema, format, duration, captures);

            static bool IsValidInstrument(ModuleSchema schema, InstrumentAudio audio)
            {
                int id = audio.InstrumentId;
                return audio.Preset
                    ? id >= 0 && id < schema.PresetInstruments.Count
                    : id >= schema.PresetInstruments.Count && id <= schema.UserSampleInstruments.Last().Id;
            }
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
