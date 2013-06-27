// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections;

namespace LinqToOperators
{
    class Comparisons
    {
        static void Main()
        {
            Compare(new[] { 1, 4, 2, 6, 3 }, new[] { 1, 4, 5 });
            Compare(new[] { "hello", "there", "world" }, new[] { "hello", "there" });
            Compare(new[] { 6, 3, 4 }, new[] { 6, 3, 4 });
        }

        static void Compare(IEnumerable lhs, IEnumerable rhs)
        {
            var evilLeft = lhs.Evil();
            Console.WriteLine("Left: {0}", evilLeft);
            Console.WriteLine("Right: {0}", rhs.Evil());
            Console.WriteLine("== {0}", evilLeft == rhs);
            Console.WriteLine("!= {0}", evilLeft != rhs);
            Console.WriteLine("> {0}", evilLeft > rhs);
            Console.WriteLine(">= {0}", evilLeft >= rhs);
            Console.WriteLine("< {0}", evilLeft < rhs);
            Console.WriteLine("<= {0}", evilLeft <= rhs);
            Console.WriteLine();
        }
    }
}
