// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    namespace EvilExtensions
    {
        static class UsDateFormat
        {
            public static void Deconstruct(this DateTime date, out int month, out int day, out int year)
            {
                day = date.Day;
                month = date.Month;
                year = date.Year;
            }
        }

        static class UkDateFormat
        {
            public static void Deconstruct(this DateTime date, out int day, out int month, out int year)
            {
                day = date.Day;
                month = date.Month;
                year = date.Year;
            }
        }
    }
}

namespace CSharp7
{
    using static EvilExtensions.UkDateFormat;

    class EvilDeconstruction
    {
        static void Main()
        {
            DateTime date = new DateTime(1976, 6, 19);
            var (x, y, z) = date;
            Console.WriteLine($"{x}/{y}/{z}");
        }
    }
}
