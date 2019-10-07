// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Audio
{
    public sealed class InstrumentAudio
    {
        /// <summary>
        /// The instrument whose audio has been captured.
        /// </summary>
        public Instrument Instrument { get; }

        /// <summary>
        /// The audio data. This should not be mutated.
        /// </summary>
        public byte[] Audio { get; }

        public InstrumentAudio(Instrument instrument, byte[] audio) =>
            (Instrument, Audio) = (instrument, audio);
    }
}
