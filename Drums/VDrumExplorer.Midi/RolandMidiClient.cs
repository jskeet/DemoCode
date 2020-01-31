// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// A MIDI client that's aware of Roland-specific system exclusive (data request, data set) messages.
    /// </summary>
    public sealed class RolandMidiClient : IDisposable
    {
        private const byte NoteOnStatus = 0x90;
        private const byte NoteOffStatus = 0x80;
        private const byte ChannelCommandStatus = 0xb0;
        private const byte AllSoundsOffCommand = 0x78;
        private const byte DataRequestCommand = 0x11;
        private const byte DataSetCommand = 0x12;

        private readonly RawMidiClient rawClient;
        private readonly byte rawDeviceId;
        private readonly int modelId;

        private readonly object sync = new object();
        private readonly LinkedList<Consumer> consumers = new LinkedList<Consumer>();

        private RolandMidiClient(RawMidiClient rawClient, byte rawDeviceId, int modelId)
        {
            this.rawClient = rawClient;
            this.rawDeviceId = rawDeviceId;
            this.modelId = modelId;
        }

        internal static async Task<RolandMidiClient> CreateAsync(MidiInputDevice inputDevice, MidiOutputDevice outputDevice, byte rawDeviceId, int modelId)
        {
            // This is all a bit nasty... we can't create a RolandMidiClient instance until we have the raw client, and we can't
            // create the raw client until we've got a method to call. LocalHandleMessage acts as a sort of trampoline.
            // If we could make the constructor asynchronous, it wouldn't be a problem.
            RolandMidiClient ret = null;
            var rawClient = await RawMidiClient.CreateAsync(inputDevice, outputDevice, LocalHandleMessage);
            ret = new RolandMidiClient(rawClient, rawDeviceId, modelId);
            return ret;

            void LocalHandleMessage(RawMidiMessage message) => ret?.HandleMessage(message);
        }

        private void HandleMessage(RawMidiMessage message)
        {
            // For the moment, we only care about SysEx Data Set messages
            if (!DataSetMessage.TryParse(message, out var result))
            {
                return;
            }
            if (result.RawDeviceId != rawDeviceId || result.ModelId != modelId)
            {
                return;
            }
            HandleDataSetMessage(result);
        }

        public void PlayNote(int channel, int note, int velocity)
        {
            var noteOnMessage = new byte[]
            {
                (byte) (NoteOnStatus | (channel - 1)),
                (byte) note,
                (byte) velocity
            };
            rawClient.Send(new RawMidiMessage(noteOnMessage));
            var noteOffMessage = new byte[]
            {
                (byte) (NoteOffStatus | (channel - 1)),
                (byte) note,
                0x64 // Fixed for NoteOff
            };
            rawClient.Send(new RawMidiMessage(noteOffMessage));
        }

        public void Silence(int channel)
        {
            var allSoundsOffMessage = new byte[]
            {
                (byte) (ChannelCommandStatus | (channel - 1)),
                AllSoundsOffCommand,
                0x0 // Fixed for AllSoundsOff
            };
            rawClient.Send(new RawMidiMessage(allSoundsOffMessage));
        }

        private void HandleDataSetMessage(DataSetMessage message)
        {
            LinkedList<TaskCompletionSource<byte[]>> sourcesToComplete = new LinkedList<TaskCompletionSource<byte[]>>();

            lock (sync)
            {
                var node = consumers.First;
                while (node != null)
                {
                    var consumer = node.Value;
                    if (consumer.ExpectedAddress == message.Address && consumer.ExpectedSize == message.Length)
                    {
                        sourcesToComplete.AddLast(consumer.TaskCompletionSource);
                        consumers.Remove(node);
                    }
                    node = node.Next;
                }
            }
            foreach (var source in sourcesToComplete)
            {
                source.TrySetResult(message.Data);
            }
        }

        /// <summary>
        /// Requests data at a given address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size">The number of bytes to receive. Must be in the range 0x1 to 0x17f inclusive.
        /// If this is in the range 0x100-0x17f, it is assumed that the actual number of bytes will be 0x80 smaller,
        /// due to the address range compression.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns></returns>
        public Task<byte[]> RequestDataAsync(int address, int size, CancellationToken cancellationToken)
        {
            if (size < 1 || size > 0x17f)
            {
                // While we could reassemble the messages, it's relatively painful to do so.
                throw new ArgumentOutOfRangeException("Size must be in the range 0x1-0x17f, to avoid fragmentation");
            }
            if (size >= 0x100)
            {
                size -= 0x80;
            }

            var consumer = new Consumer(address, size);
            lock (sync)
            {
                consumers.AddLast(consumer);
            }

            rawClient.Send(CreateDataRequestMessage(address, size));

            var tcs = consumer.TaskCompletionSource;
            return cancellationToken.CanBeCanceled ? WaitWithCancellation() : tcs.Task;

            async Task<byte[]> WaitWithCancellation()
            {
                Action cancellationAction = () =>
                {
                    lock (sync)
                    {
                        consumers.Remove(consumer);
                    }
                    tcs.TrySetCanceled();
                };
                using (cancellationToken.Register(cancellationAction))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
        }

        public void SendData(int address, byte[] bytes)
        {
            var messageData = CreateMessage(DataSetCommand, bytes.Length + 4);
            WriteBigEndianInt32(messageData, 8, address);
            Buffer.BlockCopy(bytes, 0, messageData, 12, bytes.Length);
            ApplyChecksum(messageData);
            rawClient.Send(new RawMidiMessage(messageData));
        }

        private RawMidiMessage CreateDataRequestMessage(int address, int size)
        {
            var data = CreateMessage(DataRequestCommand, 8);
            WriteBigEndianInt32(data, 8, address);
            WriteBigEndianInt28(data, 12, size);
            ApplyChecksum(data);
            return new RawMidiMessage(data);
        }

        private class Consumer
        {
            public int ExpectedAddress { get; }
            public int ExpectedSize { get; }
            public TaskCompletionSource<byte[]> TaskCompletionSource { get; }

            public Consumer(int expectedAddress, int expectedSize)
            {
                TaskCompletionSource = new TaskCompletionSource<byte[]>();
                ExpectedAddress = expectedAddress;
                ExpectedSize = expectedSize;
            }
        }


        ///// Utility methods

        /// <summary>
        /// Creates a byte array for a system exclusive message with the header information populated.
        /// </summary>
        /// <param name="dataLength">The length of data to be populated later.</param>
        private byte[] CreateMessage(byte command, int dataLength)
        {
            byte[] ret = new byte[dataLength + 10];
            ret[0] = 0xf0; // System Exclusive
            ret[1] = (byte) ManufacturerId.Roland;
            ret[2] = rawDeviceId;
            WriteBigEndianInt32(ret, 3, modelId);
            ret[7] = command;

            ret[ret.Length - 1] = 0xf7; // End of System Exclusive
            return ret;
        }

        /// <summary>
        /// Applies a Roland checksum to the SysEx message.
        /// The format of the message is:
        /// - Header info (8 bytes)
        /// - Data
        /// - Checksum (1 byte)
        /// - End message (1 byte)
        /// The checksum is applied to just the data part.
        /// </summary>
        private void ApplyChecksum(byte[] message)
        {
            byte sum = 0;
            for (int i = 8; i < message.Length - 2; i++)
            {
                sum += message[i];
            }
            message[message.Length - 2] = (byte) ((0x80 - (sum & 0x7f)) & 0x7f);
        }

        public void Dispose() => rawClient.Dispose();

        private void WriteBigEndianInt32(byte[] data, int offset, int value)
        {
            unchecked
            {
                data[offset++] = (byte) (value >> 24);
                data[offset++] = (byte) (value >> 16);
                data[offset++] = (byte) (value >> 8);
                data[offset++] = (byte) (value >> 0);
            }
        }

        /// <summary>
        /// Writes the low 28 bits of an integer across 4 bytes,
        /// leaving the top bit of each byte clear.
        /// </summary>
        private void WriteBigEndianInt28(byte[] data, int offset, int value)
        {
            unchecked
            {
                data[offset++] = (byte) ((value >> 21) & 0x7f);
                data[offset++] = (byte) ((value >> 14) & 0x7f);
                data[offset++] = (byte) ((value >> 7) & 0x7f);
                data[offset++] = (byte) ((value >> 0) & 0x7f);
            }
        }
    }
}
