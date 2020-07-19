﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model.Midi
{
    /// <summary>
    /// Abstraction of what V-Drum Explorer needs for a MIDI input connection.
    /// </summary>
    public interface IMidiInput : IDisposable
    {
        event EventHandler<MidiMessage>? MessageReceived;
    }
}
