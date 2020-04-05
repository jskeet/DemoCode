// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;

namespace VDrumExplorer.Model.Audio
{
    /// <summary>
    /// Captured audio samples for a module.
    /// </summary>
    public sealed class ModuleAudio
    {
        /// <summary>
        /// The schema of the module whose audio was sampled.
        /// </summary>
        public ModuleSchema Schema { get; }

        /// <summary>
        /// The format of all the samples.
        /// </summary>
        public AudioFormat Format { get; }

        /// <summary>
        /// The length of each sample.
        /// </summary>
        public TimeSpan DurationPerInstrument { get; }

        /// <summary>
        /// The captured audio data.
        /// </summary>
        public IReadOnlyList<InstrumentAudio> Captures { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="schema">The schema of the module whose audio was sampled.</param>
        /// <param name="format">The format of all the samples.</param>
        /// <param name="durationPerInstrument">The length of each sample.</param>
        /// <param name="captures">The captured audio data.</param>
        public ModuleAudio(ModuleSchema schema, AudioFormat format, TimeSpan durationPerInstrument, IReadOnlyList<InstrumentAudio> captures) =>
            (Schema, Format, DurationPerInstrument, Captures) = (schema, format, durationPerInstrument, captures);
    }
}
