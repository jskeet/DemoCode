// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OscMixerControl
{
    public class Mixer : IDisposable
    {
        // TODO: Should the mixer keep current values for everything? It could do, but that's a lot of data.
        
        private IOscClient client;
        private Func<IOscClient> clientProvider;
        private ConcurrentDictionary<string, EventHandler<OscMessage>> messageHandlers =
            new ConcurrentDictionary<string, EventHandler<OscMessage>>();
        
        public event EventHandler<OscPacket> PacketReceived;
        public IMixerDescriptor Descriptor { get; }

        public Mixer(IMixerDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public void Connect(string address, int port) =>
            Connect(() => new UdpOscClient(address, port));

        public void ConnectToFake() =>
            Connect(() => new FakeOscClient());

        private void Connect(Func<IOscClient> clientProvider)
        {
            this.clientProvider = clientProvider;
            Reconnect();
        }

        public void Reconnect()
        {
            if (clientProvider is null)
            {
                throw new InvalidOperationException("Cannot reconnect without having previously connected");
            }
            client?.Dispose();
            client = clientProvider();
            client.PacketReceived += HandlePacketReceived;
        }

        private void HandlePacketReceived(object sender, OscPacket packet)
        {
            PacketReceived?.Invoke(this, packet);
            if (packet is OscMessage message)
            {
                if (messageHandlers.TryGetValue(message.Address, out var handler) && handler is object)
                {
                    handler.Invoke(this, message);
                }
            }
        }

        /// <summary>
        /// Sends the given packet to the hardware mixer, optionally immediately acting as if it
        /// had been sent back by the mixer and received by this object.
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="localReflect">False (the default) if the packet should only be sent;
        /// True if this object should then respond as if the hardware mixer had sent the packet back
        /// immediately.</param>
        public async Task SendAsync(OscPacket packet, bool localReflect = false)
        {
            if (client is null)
            {
                return;
            }
            await client.SendAsync(packet);
            if (localReflect)
            {
                HandlePacketReceived(this, packet);
            }
        }

        /// <summary>
        /// Convenience method to send a simple request that just consists of the OSC
        /// address, usually as a request for data to be returned for that address.
        /// </summary>
        public Task SendDataRequestAsync(string address) =>
            SendAsync(new OscMessage(address));

        /// <summary>
        /// Returns the results of a /info enquiry, or null if no response has been returned after <paramref name="timeout"/>
        /// (or if the request cannot be sent).
        /// </summary>
        public Task<string> GetInfoAsync(TimeSpan timeout)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            EventHandler<OscMessage> handler = (sender, message) => tcs.TrySetResult(string.Join(" / ", message));
            tcs.Task.ContinueWith(task => RemoveHandler("/info", handler));
            RegisterHandler("/info", handler);
            SetNullAfterTimeout();
            var sendTask = SendInfoAsync();
            // Make sure we observe any exceptions sending the packet, but treat it as if it were a timeout.
            sendTask.ContinueWith(t =>
            {
                tcs.TrySetResult(null);
                _ = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);
            return tcs.Task;

            // Slightly hacky way of just scheduling a timeout for the task.
            async void SetNullAfterTimeout()
            {
                await Task.Delay(timeout).ConfigureAwait(false);
                tcs.TrySetResult(null);
            }
        }

        public Task SendInfoAsync() => SendAsync(new OscMessage("/info"));

        public Task SendRenewAllAsync() => SendAsync(new OscMessage("/renew"));

        public Task SendRenewAsync(string subscription) => SendAsync(new OscMessage("/renew", subscription));

        public Task SendXRemoteAsync() => SendAsync(new OscMessage("/xremote"));

        public Task SendSubscribeAsync(string address, TimeFactor timeFactor) =>
            SendAsync(new OscMessage("/subscribe", address, (int) timeFactor));

        public Task SendBatchSubscribeAsync(string alias, string address, int parameter1, int parameter2, TimeFactor timeFactor) =>
            SendAsync(new OscMessage("/batchsubscribe", alias, address, parameter1, parameter2, (int) timeFactor));

        public void RegisterHandler(string address, EventHandler<OscMessage> messageHandler) =>
            messageHandlers.AddOrUpdate(address, messageHandler, (key, existing) => existing + messageHandler);

        public void RemoveHandler(string address, EventHandler<OscMessage> messageHandler) =>
            // Annoyingly, this doesn't actually remove it from the dictionary, even if we end up with a null
            // value. That's not the end of the world; it's just a bit irritating.
            messageHandlers.AddOrUpdate(address, messageHandler, (key, existing) => existing - messageHandler);

        public void Dispose() => client?.Dispose();
    }
}
