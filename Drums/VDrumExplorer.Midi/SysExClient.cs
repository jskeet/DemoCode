// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VDrumExplorer.Midi
{
    public sealed class SysExClient : IDisposable
    {
        public int ModelId { get; }
        public int DeviceId { get; }

        private readonly InputDevice input;
        private readonly OutputDevice output;
        private readonly object sync = new object();
        private readonly LinkedList<Consumer> consumers = new LinkedList<Consumer>();
        private readonly LinkedList<SysExMessage> unconsumedMessages = new LinkedList<SysExMessage>();

        public SysExClient(int inputDeviceMidiId, int outputDeviceMidiId, int modelId, int deviceId)
        {
            input = new InputDevice(inputDeviceMidiId);
            output = new OutputDevice(outputDeviceMidiId);
            ModelId = modelId;
            DeviceId = deviceId;
            input.SysExMessageReceived += HandleSysExMessage;
            input.StartRecording();
        }

        public void PlayNote(int channel, int note, int velocity)
        {
            var builder = new ChannelMessageBuilder { MidiChannel = channel - 1, Data1 = note, Data2 = velocity, Command = ChannelCommand.NoteOn };
            builder.Build();
            output.Send(builder.Result);
            builder.Command = ChannelCommand.NoteOff;
            builder.Data2 = 0x64;
            builder.Build();
            output.Send(builder.Result);
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }

        private void HandleSysExMessage(object sender, SysExMessageEventArgs e)
        {
            if (!DataResponseMessage.TryParse(e.Message, out var response))
            {
                AddUnconsumedMessage();
                return;
            }
            if (response.DeviceId != DeviceId || response.ModelId != ModelId)
            {
                AddUnconsumedMessage();
                return;
            }
            LinkedList<TaskCompletionSource<byte[]>> sourcesToComplete = new LinkedList<TaskCompletionSource<byte[]>>();

            lock (sync)
            {
                var node = consumers.First;
                while (node != null)
                {
                    var consumer = node.Value;
                    if (consumer.ExpectedAddress == response.Address && consumer.ExpectedSize == response.Length)
                    {
                        sourcesToComplete.AddLast(consumer.TaskCompletionSource);
                        consumers.Remove(node);
                    }
                    node = node.Next;
                }
            }
            foreach (var source in sourcesToComplete)
            {
                source.TrySetResult(response.Data);
            }
            if (sourcesToComplete.Count == 0)
            {
                AddUnconsumedMessage();
            }

            void AddUnconsumedMessage()
            {
                // TODO: Limit the buffer size.
                lock (sync)
                {
                    unconsumedMessages.AddLast(e.Message);
                }
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
                // TODO: See if Sanford MIDI can do this itself.
                throw new ArgumentOutOfRangeException("Size must be in the range 0x1-0x17f, to avoid fragmentation");
            }
            if (size > 0x100)
            {
                size -= 0x80;
            }
            
            var consumer = new Consumer(address, size);
            lock (sync)
            {
                consumers.AddLast(consumer);
            }

            var request = new DataRequestMessage(DeviceId, ModelId, address, size);
            output.Send(request.ToSysExMessage());
            
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
            var message = new DataResponseMessage(DeviceId, ModelId, address, bytes);
            output.Send(message.ToSysExMessage());
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
    }
}
