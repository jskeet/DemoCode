// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class TupleFields
    {
        private (int x, int y) counters;

        static void Main()
        {
            var fields = new TupleFields();
            fields.IncrementBoth();
            fields.IncrementX();
            Console.WriteLine(fields.counters);
        }

        void IncrementBoth()
        {
            counters.x++;
            counters.y++;
        }

        void IncrementX()
        {
            counters.x++;
        }

        void IncrementY()
        {
            counters.y++;
        }
    }
}
