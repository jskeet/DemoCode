﻿// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;

namespace OscMixerControl;

internal interface IOscClient : IDisposable
{
    Task SendAsync(OscPacket packet);
    event EventHandler<OscPacket> PacketReceived;
    public Task StartReceiving();

    public class Fake : IOscClient
    {
        public event EventHandler<OscPacket>? PacketReceived;

        public void Dispose() { }

        public Task SendAsync(OscPacket packet) => Task.CompletedTask;

        public Task StartReceiving() => Task.CompletedTask;
    }
}
