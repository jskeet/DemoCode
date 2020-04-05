// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Audio
{
    /// <summary>
    /// A sample of audio data for an instrument. The format of the data is expressed separately.
    /// </summary>
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

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="instrument">The instrument whose audio has been captured.</param>
        /// <param name="audio">The audio data. This should not be mutated after construction.</param>
        public InstrumentAudio(Instrument instrument, byte[] audio) =>
            (Instrument, Audio) = (instrument, audio);
    }
}
