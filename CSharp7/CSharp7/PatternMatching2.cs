// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class PatternMatching2
    {
        static void Main()
        {
            Console.WriteLine(DoesItBlend(1L));
            Console.WriteLine(DoesItBlend(5L));
            Console.WriteLine(DoesItBlend("short"));
            Console.WriteLine(DoesItBlend("Long text"));
            Console.WriteLine(DoesItBlend(5));
            Console.WriteLine(DoesItBlend(6));
            Console.WriteLine(DoesItBlend(7));
            Console.WriteLine(DoesItBlend(null));
        }

        static string DoesItBlend(object x)
        {
            switch (x)
            {
                case 5:
                    return "5s never blend";
                case int y when (y % 5 == 0):
                case int z when (z % 5 == 0):
                    // Can't use y or z; they're not in scope
                    return "I wish I could tell you the number...";
                case int y when (y & 1) == 1:
                    Console.WriteLine(y);
                    return "Odd numbers other than 5 always blend";
                // Scoping of y is interesting here - only this case.
                case int y:
                    return "Even numbers are unreliable blenders";
                case string name when name.Length > 5:
                    return "Long text blends";
                case var o:
                    return $"Values of type {o.GetType()} may blend, or may not. I don't know.";
                // To demo: var vs default, and ordering.
            }
        }
    }
}
