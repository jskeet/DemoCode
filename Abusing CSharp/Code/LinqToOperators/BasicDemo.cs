// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class BasicDemo
    {
        static void Main(string[] args)
        {
            var funky = new[] { "hello", "world", "how", "are", "you" };
            var query = funky.Evil() - "world" + "today" & (x => x.Length == 5) | (x => x.ToUpper());

            Console.WriteLine(query * 3);

            var xor1 = new[] { "foo", "bar", "baz" }.Evil();
            var xor2 = new[] { "qux", "bar" }.Evil();
            Console.WriteLine(xor1 ^ xor2);
        }
    }
}
