// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace OscMixerControl
{
    public enum TimeFactor
    {
        /// <summary>
        /// About 200 updates over 10 seconds.
        /// </summary>
        Fast = 0,

        /// <summary>
        /// About 100 updates over 10 seconds.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// About 5 updates over 10 seconds.
        /// </summary>
        Slow = 40,

        /// <summary>
        /// About 3 updates over 10 seconds.
        /// </summary>
        VerySlow = 80
    }
}
