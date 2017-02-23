// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading;

namespace StringInterpolation
{
    class RegularEvaluation
    {
        static void Main()
        {
            string value = "Before";
            FormattableString formattable = $"Current value: {value}";
            Console.WriteLine(formattable);

            value = "After";
            Console.WriteLine(formattable);

            formattable = $"Current time: {DateTime.UtcNow:HH:mm:ss.fff}";
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(formattable);
                Thread.Sleep(1000);
            }
        }
    }   
}
