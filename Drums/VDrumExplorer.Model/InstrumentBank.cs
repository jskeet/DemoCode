// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// The "instrument bank" for an instrument: either preset instruments,
    /// or user samples.
    /// </summary>
    public enum InstrumentBank
    {
        [Description("Preset")]
        Preset,

        [Description("User samples")]
        UserSamples
    }
}
