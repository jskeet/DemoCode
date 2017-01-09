// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class OutVar
    {
        static void Main()
        {
            // Explicit typing, but still introducing variable in if
            if (int.TryParse("5", out int value))
            {
                Console.WriteLine(value);
            }            

            // var as well...
            if (int.TryParse("5", out var value2))
            {
                Console.WriteLine(value);
            }
        }
    }
}
