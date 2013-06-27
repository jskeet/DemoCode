// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class SymmetricDifference
    {
        static void Main()
        {
            var left = new[] { 1, 5, 3, 2 }.Evil();
            var right = new[] { 5, 7, 2 };
            Console.WriteLine(left ^ right);
        }
    }
}
