// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OscMixerControl
{
    public class Mixer : IDisposable
    {
        // TODO: Should the mixer keep current values for everything? It could do, but that's a lot of data.

        private IOscClient client;
        private Func<IOscClient> clientProvider;

        private ConcurrentDictionary<string, int> sentAddressCounts = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, int> receivedAddressCounts = new ConcurrentDictionary<string, int>();

        private ConcurrentDictionary<string, EventHandler<OscMessage>> messageHandlers =
            new ConcurrentDictionary<string, EventHandler<OscMessage>>();
        
        public event EventHandler<OscPacket> PacketReceived;

        public void Connect(string address, int port) =>
            Connect(() => new UdpOscClient(address, port));

        public void ConnectToFake() =>
            Connect(() => new FakeOscClient());

        public void ConnectToUiMixer(string address, int port) =>
            Connect(() => new UiOscClient(address, port));

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
                receivedAddressCounts.AddOrUpdate(message.Address, 1, (key, value) => value + 1);
                if (messageHandlers.TryGetValue(message.Address, out var handler) && handler is object)
                {
                    handler.Invoke(this, message);
                }
            }
        }

        /// <summary>
        /// Returns a snapshot of the addresses to which this mixer has sent OSC messages, with a count of packets per message.
        /// The order of the returned sequence is not specified.
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> GetSentAddressCounts() => sentAddressCounts.ToArray();
        /// <summary>
        /// Returns a snapshot of the addresses of OSC messages which this mixer has received, with a count of packets per message.
        /// The order of the returned sequence is not specified.
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> GetReceivedAddressCounts() => receivedAddressCounts.ToArray();

        public Task SendAsync(OscPacket packet)
        {
            if (packet is OscMessage message)
            {
                sentAddressCounts.AddOrUpdate(message.Address, 1, (key, value) => value + 1);
            }
            return client?.SendAsync(packet) ?? Task.CompletedTask;
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

        public Task SendXRemoteAsync() => SendAsync(new OscMessage("/xremote"));

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
