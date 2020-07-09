// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Schema.Json;

namespace VDrumExplorer.Console
{
    internal sealed class DumpDataCommand : ClientCommandBase
    {
        internal static Command Command { get; } = new Command("dump-data")
        {
            Description = "Requests data from the device, and dumps it to the console as hex",
            Handler = new DumpDataCommand(),
        }
        .AddRequiredOption<string>("--address", "Address to request (hex display format)")
        .AddRequiredOption<string>("--size", "Size of data to request (hex display format)")
        .AddOptionalOption<int>("--timeout", "Time in seconds(default to 10)", 10);

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            var address = ModuleAddress.FromDisplayValue(HexInt32.Parse(context.ParseResult.ValueForOption<string>("address")).Value);
            var size = HexInt32.Parse(context.ParseResult.ValueForOption<string>("size")).Value;
            var timeout = context.ParseResult.ValueForOption<int>("timeout");

            // Wait up to 10 seconds to receive all the requested data...
            int sizeReceived = 0;
            var delayTask = Task.Delay(TimeSpan.FromSeconds(timeout));
            var completeTaskCts = new TaskCompletionSource<int>();

            client.DataSetMessageReceived += DumpMessage;
            client.SendDataRequestMessage(address.DisplayValue, size);

            await Task.WhenAny(delayTask, completeTaskCts.Task);
            return 0;

            void DumpMessage(object sender, DataSetMessage message)
            {
                ModuleAddress address = ModuleAddress.FromDisplayValue(message.Address);
                console.WriteLine($"Address: {address} Length: {message.Length:x4}");
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
                    console.WriteLine(text);
                    address = address.PlusLogicalOffset(16);
                }
                console.WriteLine();
                if (Interlocked.Add(ref sizeReceived, message.Length) >= size)
                {
                    completeTaskCts.SetResult(0);
                }
            }
        }
    }
}
