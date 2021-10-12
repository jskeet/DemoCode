// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Threading.Tasks;

namespace OscMixerControl
{
    internal interface IOscClient : IDisposable
    {
        Task SendAsync(OscPacket packet);
        event EventHandler<OscPacket> PacketReceived;
    }
}
