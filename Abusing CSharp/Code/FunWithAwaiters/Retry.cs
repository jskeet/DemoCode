// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using System.Threading.Tasks;

namespace FunWithAwaiters
{
    class Retry
    {
        static readonly Random rng = new Random();
        
        static void Main()
        {
            RetryingExecutioner executioner = new RetryingExecutioner(ShowRetries);
            executioner.Start();
        }

        static async Task ShowRetries(RetryingExecutioner executioner)
        {
            await executioner.Setup();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Step {0}: trying to roll a 6...", i);
                await executioner.Retry(20);
                MustRoll6();
            }
            Console.WriteLine("Phew!");
        }

        static void MustRoll6()
        {
            int roll = rng.Next(1, 7);
            Console.WriteLine("Rolled {0}", roll);
            if (roll != 6)
            {
                throw new Exception("You didn't roll a 6!");
            }
        }
    }
}
