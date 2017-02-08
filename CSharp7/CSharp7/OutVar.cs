// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class OutVar
    {
        static void Main()
        {
            // Old style
            int value1;
            if (int.TryParse("5", out value1))
            {
                Console.WriteLine(value1);
            }
            // Oh noes, value1 is still in scope!
            Console.WriteLine(value1);

            // Explicit typing, but still introducing variable in if
            if (int.TryParse("5", out int value2))
            {
                Console.WriteLine(value2);
            }
            // Oh noes, value2 is still in scope too!
            Console.WriteLine(value2);

            // var as well...
            if (int.TryParse("5", out var value3))
            {
                Console.WriteLine(value3);
            }

            // Scoping is reasonable
            if (!int.TryParse("5", out int value4))
            {
                Console.WriteLine("Something is wrong");
            }
            else
            {
                Console.WriteLine($"Value was {value4}");
            }

            if (int.TryParse("foo", out int value5) && int.TryParse("bar", out int value6))
            {
                Console.WriteLine("Both parsed");
            }
            Console.WriteLine(value5);
            // Invalid because value6 isn't definitely assigned..
            // Console.WriteLine(value6);

            // But it is in scope
            value6 = 10;
        }

        public bool CheckParse(string text) => int.TryParse(text, out var _);

        public int ParseWithDefault(string text, int defaultValue) =>
            int.TryParse(text, out int value) ? value : defaultValue;
    }
}
