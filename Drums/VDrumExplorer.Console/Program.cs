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
                using (var client = CreateClientForTd17())
                {
                    int channel = 1;
                    Console.WriteLine("Playing notes");
                    client.PlayNote(channel, 36, 10);
                    Console.WriteLine("Played note 1");
                    await Task.Delay(500);

                    client.PlayNote(channel, 36, 50);
                    Console.WriteLine("Played note 2");
                    await Task.Delay(500);

                    client.PlayNote(channel, 36, 100);
                    Console.WriteLine("Played note 3");
                    await Task.Delay(500);

                    client.PlayNote(channel, 38, 10);
                    Console.WriteLine("Played note 4");
                    await Task.Delay(500);

                    client.PlayNote(channel, 38, 50);
                    Console.WriteLine("Played note 5");
                    await Task.Delay(500);

                    client.PlayNote(channel, 38, 100);
                    Console.WriteLine("Played note 6");
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static SysExClient CreateClientForTd17()
        {
            int inputId = FindId(InputDevice.DeviceCount, i => InputDevice.GetDeviceCapabilities(i).name, "3- TD-17");
            int outputId = FindId(OutputDeviceBase.DeviceCount, i => OutputDeviceBase.GetDeviceCapabilities(i).name, "3- TD-17");
            return new SysExClient(inputId, outputId, modelId: 0x4b, deviceId: 17);
        }

        private static int FindId(int count, Func<int, string> nameFetcher, string name)
        {
            for (int i = 0; i < count; i++)
            {
                if (nameFetcher(i) == name)
                {
                    return i;
                }
            }
            throw new ArgumentException($"Unable to find device {name}");
        }
    }
}
