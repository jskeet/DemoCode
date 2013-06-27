// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class Shuffle
    {
        static void Main()
        {
            var items = new[] {"who", "knows", "what", "order", "these", "words",
                               "will", "come", "out", "in?" }.Evil();

            Console.WriteLine(~items);
        }
    }
}
