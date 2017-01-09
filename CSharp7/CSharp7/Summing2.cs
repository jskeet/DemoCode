// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;

namespace CSharp7
{
    class Summing2
    {
        static void Main()
        {
            Console.WriteLine(SumAndCount(new[] { 5, 3, 1 }));
        }

        static (int sum, int count) SumAndCount(IEnumerable<int> values)
        {
            var tuple = (sum: 0, count: 0);
            foreach (var value in values)
            {
                tuple.sum += value;
                tuple.count++;
            }
            return tuple;
        }
    }
}
