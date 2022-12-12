// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace OscMixerControl;

internal static class LevelConversions
{
    /// <summary>
    /// Converts dB (from output, i.e. expected to be -inf to 0) to
    /// a linear value in the range 0-1, suitable for display.
    /// </summary>
    internal static double OutputDbToLinear(double db)
    {
        // Historically we scaled to 0-600. We can adjust this later...
        double scaled = db switch
        {
            >= -5f => (db + 5f) * (100f / 5f) + 500,
            >= -10f => (db + 10f) * (100f / 5f) + 400,
            >= -20f => (db + 20f) * (100f / 10f) + 300,
            >= -30f => (db + 30f) * (100f / 10f) + 200,
            >= -50f => (db + 50f) * (100f / 20f) + 100,
            >= -75f => (db + 75f) * (100f / 25) + 0,
            _ => 0f
        };

        return scaled / 600.0;
    }
}
