// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
#define ISO_DATE_FORMAT
using System;

namespace OddsAndEnds.DateFormatting
{
    static class DateTimeExtensions
    {
#if UK_DATE_FORMAT
        public static void Deconstruct(this DateTime value, out int day, out int month, out int year)
#elif US_DATE_FORMAT
        public static void Deconstruct(this DateTime value, out int month, out int day, out int year)
#elif ISO_DATE_FORMAT
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

    static class UkDateFormat
    {
        public static void Deconstruct(this DateTime value, out int day, out int month, out int year)
        {
            day = value.Day;
            month = value.Month;
            year = value.Year;
        }
    }

    static class UsDateFormat
    {
        public static void Deconstruct(this DateTime value, out int month, out int day, out int year)
        {
            day = value.Day;
            month = value.Month;
            year = value.Year;
        }
    }

    static class IsoDateFormat
    {
        public static void Deconstruct(this DateTime value, out int year, out int month, out int day)
        {
            day = value.Day;
            month = value.Month;
            year = value.Year;
        }
    }
}
