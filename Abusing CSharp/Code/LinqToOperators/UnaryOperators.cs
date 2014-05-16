// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace LinqToOperators
{
    class UnaryOperators
    {
        static void Main()
        {
            var sequence = new[] { 3, 1, 4, 1, 5, 9, 2 }.Evil();

            Console.WriteLine("Original: {0}", sequence);
            Console.WriteLine("Reversed: {0}", -sequence);
            Console.WriteLine("Negated: {0}", -+sequence);
            Console.WriteLine("Mixture: {0}", -+-sequence);
            Console.WriteLine("Pop:");
            var popping = + +sequence;
            Console.WriteLine(-popping);
            Console.WriteLine(-popping);
            Console.WriteLine(-popping);
            Console.WriteLine("After popping: {0}", popping);

            Console.WriteLine("Reversed original via cycle: {0}", -+ + +sequence);
        }
    }
}
