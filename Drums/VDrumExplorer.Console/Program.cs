// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Sanford.Multimedia.Midi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;

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
                var myInput = inputs.Single(input => input.Name == "2- TD-17");
                var myOutput = outputs.Single(output => output.Name == "2- TD-17");
                var identities = await MidiDevices.ListDeviceIdentities(myInput, myOutput, TimeSpan.FromSeconds(0.5));
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
    }
}
