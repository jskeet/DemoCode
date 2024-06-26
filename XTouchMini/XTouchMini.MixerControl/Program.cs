﻿// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using OscMixerControl;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTouchMini.Model;

namespace XTouchMini.MixerControl
{
    class Program
    {
        static async Task Main()
        {
            // MIDI input name hard-coded as I think the chances of anyone else running this are slim...
            await using (var controller = await XTouchMiniMackieController.ConnectAsync(NullLogger.Instance, "X-TOUCH MINI"))
            {
                Console.WriteLine("Connected to controller");

                using (var mixer = new Mixer(XAirDescriptor.Instance))
                {
                    // IP address hard-coded as I think the chances of anyone else running this are slim...
                    mixer.Connect("192.168.1.41", 10024);
                    mixer.RegisterHandler("/info", (sender, message) => Console.WriteLine($"Mixer info response: {string.Join("/", message)}"));
                    await mixer.SendInfoAsync();

                    using (var connector = new MixerConnector(controller, mixer))
                    {
                        await connector.StartAsync();
                        await Task.Delay(Timeout.Infinite);
                    }
                }
            }
        }
    }
}
