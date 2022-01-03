// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Threading;

namespace DmxLighting.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var universe = new DmxUniverse(0, 300);
            var sender = new StreamingAcnSender("192.168.1.46");
            sender.SendUniverse(universe);

            Thread.Sleep(2000);

            byte[] off = { 0, 0, 0 };
            byte[][] colours =
            {
                new byte[] { 255, 0, 0 },
                new byte[] { 0, 255, 0 },
                new byte[] { 0, 0, 255 },
                new byte[] { 255, 255, 0 },
                new byte[] { 0, 255, 255 },
                new byte[] { 255, 255, 255 },
            };

            foreach (byte[] colour in colours)
            {
                for (int i = 0; i < 71; i++)
                {
                    universe.SetChannelValues(i * 3 + 1, colour);
                    sender.SendUniverse(universe);
                    Thread.Sleep(40);
                    universe.SetChannelValues(i * 3 + 1, off);
                }
                for (int i = 70; i > 0; i--)
                {
                    universe.SetChannelValues(i * 3 + 1, colour);
                    sender.SendUniverse(universe);
                    Thread.Sleep(40);
                    universe.SetChannelValues(i * 3 + 1, off);
                }
            }
            sender.SendUniverse(universe);

            //sender.WatchUniverse(universe);
            universe[1] = 0;
            /*
            for (int i = 1; i <= 213; i++)
            {
                universe[i] = 127;
                //sender.SendUniverse(universe);
                Thread.Sleep(20);
                universe[i] = 255;
                Thread.Sleep(20);
                //sender.SendUniverse(universe);
                //universe[i] = 0;
                //Thread.Sleep(100);
            }*/
        }

        static void FusionAlvor()
        {
            // 1-37 = Fusion Orbit
            // 38-43 = Latta Alvor
            var universe = new DmxUniverse(0, 43);

            
            var sender = new StreamingAcnSender("192.168.1.46");

            // Latta Alvor:
            // Manual mode, full total brightness, with half red/blue and full green
            universe.SetChannelValues(38, new byte[] { 255, 127, 255, 127, 0, 0 });

            // Fusion orbit
            // Channels 1-5 are for pan/tilt; leave at 0.
            universe[6] = 255;   // Master dimmer: full brightness

            // Beam
            universe[7] = 0;     // No strobe
            universe[8] = 0;     // No colour macro
            universe[9] = 127;   // Red
            universe[10] = 255;  // Green
            universe[11] = 127;  // Blue
            universe[12] = 31;   // White

            // LED Ring
            universe[13] = 255;  // Master dimmer: full brightness
            universe[14] = 0;    // No strobe
            universe[15] = 0;    // No colour macro
            universe[16] = 0;    // No colour program
            universe[17] = 0;    // No macro progam speed

            universe[18] = 255;  // Segment 1: all red
            universe[22] = 255;  // Segment 2: all green
            universe[26] = 255;  // Segment 3: all blue

            universe[26] = 127;  // Segment 4: half-brightness R+G+B
            universe[27] = 127;
            universe[28] = 127;

            universe[29] = 31;   // Segment 5: dull red
            universe[34] = 31;   // Segment 6: dull green

            universe[36] = 0;    // No "show"
            universe[37] = 0;    // No sound sensitivity

            sender.SendUniverse(universe);

            // Pan to max
            Console.ReadLine();
            universe[1] = 255;
            sender.SendUniverse(universe);

            // Tilt to max
            Console.ReadLine();
            universe[3] = 255;
            sender.SendUniverse(universe);

            // Reset back, with all red
            Console.ReadLine();
            universe[1] = 0;
            universe[3] = 0;
            universe[9] = 255;
            universe[10] = 0;
            universe[11] = 0;
            universe[12] = 0;

            for (int ring = 0; ring < 6; ring++)
            {
                universe[ring * 3 + 18] = 255;
                universe[ring * 3 + 19] = 0;
                universe[ring * 3 + 20] = 0;
            }
            sender.SendUniverse(universe);

            // Move it around a bit
            Console.ReadLine();
            universe.SetChannelValues(38, new byte[] { 255, 0, 0, 255, 0, 0 });
            universe[1] = 10;
            universe[3] = 10;
            sender.SendUniverse(universe);
            Thread.Sleep(2000);

            universe.SetChannelValues(38, new byte[] { 255, 0, 255, 0, 0, 0 });
            universe[1] = 30;
            universe[3] = 30;
            sender.SendUniverse(universe);
            Thread.Sleep(2000);

            universe.SetChannelValues(38, new byte[] { 255, 255, 0, 0, 0, 0 });
            universe[1] = 40;
            universe[3] = 0;
            sender.SendUniverse(universe);

            // Turn everything off
            Console.ReadLine();
            for (int i = 1; i <= 43; i++)
            {
                universe[i] = 0;
            }
            sender.SendUniverse(universe);
        }
    }
}
