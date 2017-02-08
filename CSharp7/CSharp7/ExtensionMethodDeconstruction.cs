// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using NodaTime;
using NodaTime.Extensions;
using System;

namespace CSharp7
{
    class ExtensionMethodDeconstruction
    {
        static void Main()
        {
            var clock = SystemClock.Instance.InUtc();
            LocalDate today = clock.GetCurrentDate();

            // Different names just for clarity...
            var (y, m, d) = today;
            Console.WriteLine($"Year: {y}; Month: {m}; Day: {d}");
        }
    }

    static class LocalDateExtensions
    {
        public static void Deconstruct(this LocalDate date, out int year, out int month, out int day)
        {
            year = date.Year;
            month = date.Month;
            day = date.Day;
        }
    }
}
