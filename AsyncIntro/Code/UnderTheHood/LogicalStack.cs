// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace UnderTheHood
{
    class LogicalStack
    {
        public static async Task DemonstrateStacks()
        {
            int y = 10;
            Task<int> z = Task.FromResult(10);
            var x = y * await z;

            Task<int> task = Task.FromResult(20);
            Console.WriteLine("{0} {1}", x, await task);
        }
    }
}
