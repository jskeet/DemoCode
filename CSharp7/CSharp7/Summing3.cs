// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp7
{
    class Summing3
    {
        static void Main()
        {
            Console.WriteLine(SumAndCount(new[] { 5, 3, 1 }));
        }

        static (int sum, int count) SumAndCount(IEnumerable<int> values)
            => values.Aggregate(
                seed: (sum: 0, count: 0),
                func: (tuple, value) => (tuple.sum + value, tuple.count + 1));
    }
}
