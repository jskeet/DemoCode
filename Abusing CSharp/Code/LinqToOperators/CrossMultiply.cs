// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class CrossMultiply
    {
        static void Main()
        {
            var input1 = new[] {"a", "b", "c"}.Evil();
            var input2 = new[] {1, 2, 3};

            Console.WriteLine(input1 * input2);
        }
    }
}
