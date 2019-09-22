// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
#pragma warning disable CS0219 // Variable is assigned but its value is never used
using System;

namespace CSharp7
{
    class TupleConstruction
    {
        static void Main()
        {
            var tuple1 = (1, 2);
            Console.WriteLine($"Tuple 1: item 1 = {tuple1.Item1}, item 2 = {tuple1.Item2}");

            var tuple2 = new ValueTuple<int, long>(1, 2);
            Console.WriteLine($"Tuple 2: item 1 = {tuple2.Item1}, item 2 = {tuple2.Item2}");
            
            var tuple3 = (a: 1, b: 2);
            Console.WriteLine($"Tuple 3: a = {tuple3.a}, b = {tuple3.b}");

            // Names specified by declaration
            (long a, int b) tuple4 = (1, 2);
            Console.WriteLine($"Tuple 4: a = {tuple4.a}, b = {tuple4.b}");

            // No names in declaration, so names in construction are irrelevant, warning will be given
            #pragma warning disable CS8123
            (int, int) tuple5 = (a: 1, b: 2);
            Console.WriteLine($"Tuple 5: item 1 = {tuple5.Item1}, item 2 = {tuple5.Item2}");
            #pragma warning restore CS8123

            // Names in declaration, then assign by name
            (int id, string name, DateTime created) tuple6 = (id: 1, name: "Some name", created: DateTime.UtcNow);
            Console.WriteLine($"Tuple 6: id = {tuple6.id}, name = '{tuple6.name}', created = {tuple6.created}");
        }
    }
}
