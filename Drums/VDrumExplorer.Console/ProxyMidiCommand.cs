// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    internal sealed class ProxyMidiCommand : AsynchronousCommandLineAction
    {
        internal static Command Command { get; } = new Command("proxy-midi")
        {
            Description = "Proxies MIDI notes from an input to an output",
            Action = new ProxyMidiCommand()
        }
        .AddRequiredOption<string>("--input", "Input MIDI name")
        .AddRequiredOption<string>("--output", "Output MIDI name")
        .AddOptionalOption("--inputChannel", "Input channel to map", -1)
        .AddOptionalOption("--outputChannel", "Output channel to map", -1);

        public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            var console = parseResult.InvocationConfiguration.Output;
            var inputDevices = MidiDevices.ListInputDevices();

            string inputName = parseResult.GetRequiredValue<string>("input");
            var inputDevice = inputDevices.FirstOrDefault(d => d.Name == inputName);
            if (inputDevice is null)
            {
                console.WriteLine($"Input device '{inputName}' not found.");
                return 1;
            }

            var outputDevices = MidiDevices.ListOutputDevices();
            string outputName = parseResult.GetRequiredValue<string>("output");
            var outputDevice = outputDevices.FirstOrDefault(d => d.Name == outputName);
            if (outputDevice is null)
            {
                console.WriteLine($"Output device '{outputName}' not found.");
                return 1;
            }

            int inputChannel = parseResult.GetRequiredValue<int>("inputChannel");
            int outputChannel = parseResult.GetRequiredValue<int>("outputChannel");

            using var input = await MidiDevices.Manager.OpenInputAsync(inputDevice);
            using var output = await MidiDevices.Manager.OpenOutputAsync(outputDevice);

            console.WriteLine("Proxying");
            input.MessageReceived += (sender, message) =>
            {
                var type = message.Status & 0xf0;
                switch (type)
                {
                    case 0b1000_0000: // Note off
                    case 0b1001_0000: // Note on
                    case 0b1010_0000: // Polyphonic key pressure (aftertouch)
                    case 0b1101_0000: // Channel pressure (aftertouch)
                    case 0b1110_0000: // Pitch bend change
                        if ((message.Status & 0xf) == inputChannel)
                        {
                            message.Data[0] = (byte) (type | outputChannel);
                        }
                        output.Send(message);
                        break;
                    case 0b1011: // Control change
                    case 0b1100: // Program change
                    case 0b1111: // System exclusive
                        break;
                }
            };

            // Effectively "forever unless I forget to turn things off".
            await Task.Delay(TimeSpan.FromHours(1));
            return 0;
        }
    }
}
