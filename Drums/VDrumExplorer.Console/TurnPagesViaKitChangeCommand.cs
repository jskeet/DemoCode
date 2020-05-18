// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Command for turning pages with a foot switch. The foot switch should
    /// be set to increment the kit number. This command:
    /// 
    /// - Detects the device
    /// - Fetches the current kit number
    /// - Loads the configuration for that kit
    /// - Copies that configuration to two contiguous kits (99 and 100 by default)
    /// - Sets the current kit number to 99
    /// - Starts listening for MIDI events
    /// - When a MIDI Program Change event is received, send keys to the current
    ///   application to turn the page, and reset the current kit to 99.
    /// </summary>
    public class TurnPagesViaKitChangeCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("turn-pages-kit")
        {
            Description = "Performs page turning when the kit changes (e.g. via foot switch)",
            Handler = new TurnPagesViaKitChangeCommand(),
        }
        .AddOptionalOption("--channel", "MIDI channel", 10)
        .AddOptionalOption("--keys", "SendKeys key string", "{RIGHT}")
        .AddOptionalOption("--kit", "Target kit to adopt (and the following one)", 99);

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;

            if (!SendKeysUtilities.HasSendKeys)
            {
                console.WriteLine($"SendKeys not detected; this command can only be run on Windows.");
                return 1;
            }

            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(new ConsoleLogger(console), SchemaRegistry.KnownSchemas.Keys);
            if (client is null)
            {
                return 1;
            }
            var schema = SchemaRegistry.KnownSchemas[client.Identifier].Value;

            using (client)
            {

                var channel = context.ParseResult.ValueForOption<int>("channel");
                var keys = context.ParseResult.ValueForOption<string>("keys");
                var targetKit = context.ParseResult.ValueForOption<int>("kit");
                if (targetKit < 1 || targetKit + 1 > schema.KitRoots.Count)
                {
                    console.WriteLine($"Kit {targetKit} is out of range for {schema.Identifier.Name} for this command.");
                    console.WriteLine("Note that one extra kit is required after the specified one.");
                    return 1;
                }

                // Detect the current kit
                // TODO: Stop assuming current kit is at address 0, although it is for TD-17, TD-50 and TD-27...
                var data = await client.RequestDataAsync(0, 1, new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);
                var currentKit = data[0] + 1;

                // Copy current kit to target kit and target kit + 1
                var kit = await KitUtilities.ReadKit(schema, client, currentKit, console);
                await KitUtilities.WriteKit(client, kit, targetKit, console);
                await KitUtilities.WriteKit(client, kit, targetKit + 1, console);

                SetCurrentKit(targetKit);
                var programChangeCommand = (byte) (0xc0 | (channel - 1));

                // Now listen for the foot switch...
                client.MessageReceived += (sender, message) =>
                {
                    if (message.Data.Length == 2 && message.Data[0] == programChangeCommand)
                    {
                        console.WriteLine("Turning the page...");
                        SendKeysUtilities.SendWait(keys);
                        SetCurrentKit(targetKit);
                    }
                };
                console.WriteLine("Listening for foot switch");
                await Task.Delay(TimeSpan.FromHours(1));
            }
            return 0;

            void SetCurrentKit(int newKitNumber) => client.SendData(0, new[] { (byte) (newKitNumber - 1) });
        }
    }
}
