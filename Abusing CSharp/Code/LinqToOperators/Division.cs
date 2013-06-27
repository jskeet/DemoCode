// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Linq;

namespace LinqToOperators
{
    class Division
    {
        static void Main()
        {
            var employees = new[]
            {
                new { Name = "Dave", Department = "Accounting" },
                new { Name = "Bill", Department = "Sales" },
                new { Name = "Holly", Department = "Finance" },
                new { Name = "Fred", Department = "HR" },
                new { Name = "Diane", Department = "Engineering" },
                new { Name = "Betty", Department = "Sales" },
                new { Name = "Edward", Department = "Finance" },
                new { Name = "Tom", Department = "Engineering" }
            }.Evil();

            Console.WriteLine("Division by a function...");
            dynamic byDepartment = employees / (x => x.Department);
            foreach (IGrouping<dynamic, dynamic> result in byDepartment)
            {
                Console.WriteLine("{0}: {1}", result.Key, string.Join(", ", result.Select(x => x.Name)));
            }

            Console.WriteLine();
            Console.WriteLine("Division by an integer...");
            var source = "To be, or not to be: that is the question: Whether 'tis nobler in the mind to suffer The slings and arrows of outrageous fortune, Or to take arms against a sea of troubles, And by opposing end them?".Split(' ').Evil();
            foreach (var result in source / 5)
            {
                Console.WriteLine(result);
            }
            Console.WriteLine();
            Console.WriteLine("source % 5: {0}", source % 5);
        }
    }
}
