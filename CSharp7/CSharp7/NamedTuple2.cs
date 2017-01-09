// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class NamedTuple2
    {
        static void Main()
        {
            // "Target typing"
            (int x, int y, string z) tuple = CreateTuple();
            Console.WriteLine(tuple.x);
            Console.WriteLine(tuple.y);
            Console.WriteLine(tuple.z);
        }

        static (int, int, string) CreateTuple()
        {
            return (5, 3, "hello");
        }
    }
}
