// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class RefReturn
    {
        static void Main(string[] args)
        {
            var rng = new Random();
            int a = 0, b = 0;
            for (int i = 0; i < 20; i++)
            {
                // Increments one of the variables
                GetRandomVariable(rng, ref a, ref b)++;

                // This has no effect; x is inferred to be an int,
                // and gets a copy of the value of a or b, the
                // increment does not affect a or b.
                var x = GetRandomVariable(rng, ref a, ref b);
                x++;

                Console.WriteLine($"a={a}, b={b}");
            }
        }

        static ref int GetRandomVariable(Random rng, ref int x, ref int y)
        {
            // Doesn't compile: conditional operator and ref local don't go together
            // return rng.NextDouble() >= 0.5 ? ref x : ref y;

            if (rng.NextDouble() >= 0.5)
            {
                return ref x;
            }
            else
            {
                return ref y;
            }            
        }
    }
}
