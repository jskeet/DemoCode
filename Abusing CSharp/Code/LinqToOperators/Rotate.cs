// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class Rotate
    {
        static void Main()
        {
            var source = "the quick brown fox jumps over the lazy dog".Split(' ').Evil();

            Console.WriteLine(source);
            Console.WriteLine(source << 2);
            Console.WriteLine(source >> 2);
        }
    }
}
