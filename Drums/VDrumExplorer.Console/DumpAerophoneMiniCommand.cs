// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Schema.Json;

namespace VDrumExplorer.Console
{
    internal sealed class DumpAerophoneMiniCommand : ClientCommandBase
    {
        internal static Command Command { get; } = new Command("dump-aerophone-mini")
        {
            Description = "Requests known Aerophone Mini segments from the device, and dumps it to a file as hex",
            Handler = new DumpAerophoneMiniCommand(),
        }
        .AddRequiredOption<string>("--file", "File to write to");

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            using var writer = File.CreateText(context.ParseResult.ValueForOption<string>("file"));
            int chunkSize = 128;
            TimeSpan timeout = TimeSpan.FromMilliseconds(200);

            int[] displayValues =
            {
                0x00273200,
                0x00273300,
                0x00273400,
                0x00350000,
                0x00420000,
                0x00510000
            };

            int messagesReceived = 0;
            client.DataSetMessageReceived += DumpMessage;

            foreach (var displayValue in displayValues)
            {
                ModuleAddress address = ModuleAddress.FromDisplayValue(displayValue);
                int receivedAtStartOfChunk = messagesReceived;
                client.SendDataRequestMessage(address.DisplayValue, chunkSize);

                int receivedAtStartOfDelay;
                do
                {
                    receivedAtStartOfDelay = messagesReceived;
                    await Task.Delay(timeout);
                } while (messagesReceived > receivedAtStartOfDelay);

                int receivedForChunk = messagesReceived - receivedAtStartOfChunk;

                console.WriteLine($"Received {receivedForChunk} messages in chunk at {address}");
                writer.Flush();
                address = address.PlusLogicalOffset(chunkSize);
            }

            return 0;

            void DumpMessage(object sender, DataSetMessage message)
            {
                ModuleAddress address = ModuleAddress.FromDisplayValue(message.Address);
                writer.WriteLine($"Address: {address} Length: {message.Length:x4}");
                int index = 0;
                while (index < message.Length)
                {
                    var builder = new StringBuilder();
                    var textBuilder = new StringBuilder();
                    builder.Append(address);
                    builder.Append(" ");
                    for (int i = 0; i < 16 && index < message.Length; i++)
                    {
                        byte b = message.Data[index];
                        textBuilder.Append(b >= 32 && b < 127 ? (char) b : ' ');
                        builder.Append(b.ToString("x2"));
                        builder.Append(" ");
                        index++;
                    }
                    string text = builder.ToString().PadRight(9 + 16 * 3) + textBuilder;
                    writer.WriteLine(text);
                    address = address.PlusLogicalOffset(16);
                }
                writer.WriteLine();
                Interlocked.Increment(ref messagesReceived);
            }
        }
    }
}
