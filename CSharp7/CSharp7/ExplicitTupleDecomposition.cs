// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class ExplicitTupleDecomposition
    {
        static void Main()
        {
            var (a, b, c) = CreateTuple();
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
        }

        static (int x, int y, string z) CreateTuple()
        {
            return (5, 3, "hello");
        }
    }
}
