// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class TupleParameters
    {
        static void Main()
        {
            (int foo, int bar) result = Add((5, 3), (6, 4));
            Console.WriteLine(result.foo);
            Console.WriteLine(result.bar);
            Console.WriteLine(result);
        }

        static (int a, int b) Add((int a, int b) x, (int, int) y)
        {
            return (x.a + y.Item1, x.b + y.Item2);
        }
    }
}
