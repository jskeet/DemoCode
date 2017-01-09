// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

#define UK_DATES

using System;

namespace OddsAndEnds
{
    static class DateTimeExtensions
    {
#if UK_DATES
        public static void Deconstruct(this DateTime value, out int day, out int month, out int year)
#elif US_DATES
        public static void Deconstruct(this DateTime value, out int month, out int day, out int year)
#elif ISO_DATES
        public static void Deconstruct(this DateTime value, out int year, out int month, out int day)
#else
#error No date format defined
#endif
        {
            day = value.Day;
            month = value.Month;
            year = value.Year;
        }
    }
}
