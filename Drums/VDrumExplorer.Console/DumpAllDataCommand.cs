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
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Schema.Json;

namespace VDrumExplorer.Console
{
    internal sealed class DumpAllDataCommand : ClientCommandBase
    {
        internal static Command Command { get; } = new Command("dump-all-data")
        {
            Description = "Requests all data from the device, and dumps it to a file as hex",
            Handler = new DumpAllDataCommand(),
        }
        .AddRequiredOption<string>("--file", "File to write to")
        .AddOptionalOption("--chunkSize", "Size of chunk", 0x4000)
        .AddOptionalOption("--timeout", "Idle timeout (ms)", 200);

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            using var writer = File.CreateText(context.ParseResult.ValueForOption<string>("file"));
            int chunkSize = context.ParseResult.ValueForOption<int>("chunkSize");
            TimeSpan timeout = TimeSpan.FromMilliseconds(context.ParseResult.ValueForOption<int>("timeout"));

            var address = ModuleAddress.FromDisplayValue(0);
            int messagesReceived = 0;
            client.DataSetMessageReceived += DumpMessage;

            // TODO: Make this tighter. But we're unlikely to really get here.
            while (address.DisplayValue < 0x74_00_00_00)
            {
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
