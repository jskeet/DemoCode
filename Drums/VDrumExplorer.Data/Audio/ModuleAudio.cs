// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;
using VDrumExplorer.Data.Proto;

namespace VDrumExplorer.Data.Audio
{
    public sealed class ModuleAudio
    {
        public ModuleSchema Schema { get; }
        public AudioFormat Format { get; }
        public TimeSpan DurationPerInstrument { get; }
        public IReadOnlyList<InstrumentAudio> Captures { get; }

        public ModuleAudio(ModuleSchema schema, AudioFormat format, TimeSpan durationPerInstrument, IReadOnlyList<InstrumentAudio> captures) =>
            (Schema, Format, DurationPerInstrument, Captures) = (schema, format, durationPerInstrument, captures);

        public void Save(Stream stream) => ProtoIo.Write(stream, this);
    }
}
