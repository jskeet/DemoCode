// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Midi;

namespace VDrumExplorer.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Just so we can write synchronous code when we want to
            await Task.Yield();

            try
            {
                var inputs = MidiDevices.ListInputDevices();
                foreach (var input in inputs)
                {
                    Console.WriteLine($"Input: {input}");
                }
                var outputs = MidiDevices.ListOutputDevices();
                foreach (var output in outputs)
                {
                    Console.WriteLine($"Output: {output}");
                }
                var td17Input = inputs.Single(input => input.Name == "2- TD-17");
                var td17Output = outputs.Single(output => output.Name == "2- TD-17");

                var identities = await MidiDevices.ListDeviceIdentities(td17Input, td17Output, TimeSpan.FromSeconds(0.5));
                foreach (var identity in identities)
                {
                    Console.WriteLine(identity);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Input_MessageReceived(object sender, MidiReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
