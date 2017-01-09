// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class UnnamedTuple
    {
        static void Main()
        {
            (int, int, string) tuple = CreateTuple();
            Console.WriteLine(tuple.Item1);
            Console.WriteLine(tuple.Item2);
            Console.WriteLine(tuple.Item3);
        }

        static (int, int, string) CreateTuple()
        {
            return ValueTuple.Create(5, 3, "hello");
        }
    }
}
