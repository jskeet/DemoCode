// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Linq;

namespace LinqToOperators
{
    class Inverse
    {
        static void Main()
        {
            var sequence = new[] { 3, 1, 4, 1, 5, 9, 2 }.Evil();
            var inverse = !sequence;
            var range = Enumerable.Range(0, 10).Evil();
            Console.WriteLine(range + inverse);
        }
    }
}
