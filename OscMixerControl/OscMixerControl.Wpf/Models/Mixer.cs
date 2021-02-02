// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OscMixerControl.Wpf.Models
{
    public class Mixer
    {
        // TODO: Should the mixer keep current values for everything? It could do...

        private UdpOscClient client;
        private ConcurrentDictionary<string, EventHandler<OscMessage>> messageHandlers =
            new ConcurrentDictionary<string, EventHandler<OscMessage>>();
        
        public event EventHandler<OscPacket> PacketReceived;

        public void Connect(string address, int port)
        {
            client?.Dispose();
            client = new UdpOscClient(address, port);
            client.PacketReceived += HandlePacketReceived;
        }

        private void HandlePacketReceived(object sender, OscPacket packet)
        {
            PacketReceived?.Invoke(this, packet);
            if (packet is OscMessage message)
            {
                if (messageHandlers.TryGetValue(message.Address, out var handler))
                {
                    handler.Invoke(this, message);
                }
            }
        }

        public Task SendAsync(OscPacket packet) =>
            client?.SendAsync(packet) ?? Task.CompletedTask;

        /// <summary>
        /// Convenience method to send a simple request that just consists of the OSC
        /// address, usually as a request for data to be returned for that address.
        /// </summary>
        public Task SendDataRequestAsync(string address) =>
            SendAsync(new OscMessage(address));

        public Task SendRenewAllAsync() => SendAsync(new OscMessage("/renew"));

        public Task SendRenewAsync(string subscription) => SendAsync(new OscMessage("/renew", subscription));

        public Task SendXRemoteAsync() => SendAsync(new OscMessage("/xremote"));

        internal Task SendSubscribeAsync(string address, TimeFactor timeFactor) =>
            SendAsync(new OscMessage("/subscribe", address, (int) timeFactor));

        internal Task SendBatchSubscribeAsync(string alias, string address, int parameter1, int parameter2, TimeFactor timeFactor) =>
            SendAsync(new OscMessage("/batchsubscribe", alias, address, parameter1, parameter2, (int) timeFactor));

        internal void RegisterHandler(string muteAddress, EventHandler<OscMessage> messageHandler)
        {
            messageHandlers.AddOrUpdate(muteAddress, messageHandler, (key, existing) => existing + messageHandler);
        }
    }
}
