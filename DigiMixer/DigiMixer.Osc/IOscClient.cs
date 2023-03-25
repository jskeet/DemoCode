// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;

namespace DigiMixer.Osc;

internal interface IOscClient : IDisposable
{
    Task SendAsync(OscPacket packet);
    event EventHandler<OscPacket> PacketReceived;
    void Start();

    public class Fake : IOscClient
    {
        internal static Fake Instance { get; } = new Fake();
        private Fake() { }

        public event EventHandler<OscPacket>? PacketReceived
        {
            add { }
            remove { }
        }

        public void Dispose() { }

        public Task SendAsync(OscPacket packet) => Task.CompletedTask;

        public void Start() { }
    }
}
