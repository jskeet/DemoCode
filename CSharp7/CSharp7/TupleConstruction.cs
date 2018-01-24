// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
#pragma warning disable CS0219 // Variable is assigned but its value is never used
using System;

namespace CSharp7
{
    class TupleConstruction
    {
        static void Main()
        {
            var tuple1 = (1, 2);
            Console.WriteLine(tuple1.Item1);
            Console.WriteLine(tuple1.Item2);

            var tuple2 = new ValueTuple<int, long>(1, 2);
            Console.WriteLine(tuple2.Item1);
            Console.WriteLine(tuple2.Item2);
            
            var tuple3 = (a: 1, b: 2);
            Console.WriteLine(tuple3.a);
            Console.WriteLine(tuple3.b);

            // Names specified by declaration
            (long a, int b) tuple4 = (1, 2);

            // No names in declaration, so names in construction are irrelevant
            (int, int) tuple5 = (a: 1, b: 2);
        }
    }
}
