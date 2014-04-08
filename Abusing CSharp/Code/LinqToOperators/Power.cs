// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class Power
    {
        // As suggested by Nat Pryce at NorDevCon, 2014-02-28
        static void Main()
        {
            var source = new[] { "foo", "bar", "baz" }.Evil();

            var cubed = source ^ 3;
            Console.WriteLine(cubed);
        }
    }
}
