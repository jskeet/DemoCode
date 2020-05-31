// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;

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
    public class TurnPagesViaMidiCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("turn-pages-note")
        {
            Description = "Performs page turning when the specified note is played (e.g. an AUX pad)",
            Handler = new TurnPagesViaMidiCommand(),
        }
        .AddOptionalOption("--channel", "MIDI channel", 10)
        .AddOptionalOption("--note", "MIDI note to listen for", 32)
        .AddOptionalOption("--keys", "SendKeys key string", "{RIGHT}");

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            if (!SendKeysUtilities.HasSendKeys)
            {
                console.WriteLine($"SendKeys not detected; this command can only be run on Windows.");
                return 1;
            }

            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(new ConsoleLogger(console), ModuleSchema.KnownSchemas.Keys);
            if (client is null)
            {
                return 1;
            }
            var schema = ModuleSchema.KnownSchemas[client.Identifier].Value;

            using (client)
            {

                var channel = context.ParseResult.ValueForOption<int>("channel");
                var keys = context.ParseResult.ValueForOption<string>("keys");
                var midiNote = context.ParseResult.ValueForOption<int>("note");

                var noteOn = (byte) (0x90 | (channel - 1));

                // Now listen for the foot switch...
                client.MessageReceived += (sender, message) =>
                {
                    if (message.Data.Length == 3 && message.Data[0] == noteOn && message.Data[1] == midiNote)
                    {
                        console.WriteLine("Turning the page...");
                        SendKeysUtilities.SendWait(keys);
                    }
                };
                console.WriteLine("Listening for MIDI note");
                await Task.Delay(TimeSpan.FromHours(1));
            }
            return 0;
        }
    }
}
