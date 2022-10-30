// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace EvChargerTiming;

public class EvCharger
{
    /// <summary>
    /// Whether or not the charger is currently "on" in terms of
    /// being able to supply power. Somewhat orthogonally, the real
    /// system would be able to report whether a car was actually plugged
    /// in or not, how much current was being drawn etc.
    /// </summary>
    public bool On { get; private set; }

    /// <summary>
    /// Method to change to the given state.
    /// (This could be a property setter, or two different methods...
    /// all kinds of different design options here which aren't relevant
    /// to the point of this sample code.)
    /// </summary>
    public void ChangeState(bool on)
    {
        // Probably some logging here...
        On = on;
    }
}
