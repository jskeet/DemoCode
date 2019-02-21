using System;

namespace PatternsAndRanges
{
    static class DateTimeExtensions
    {
        public static void Deconstruct(this DateTime date, out int year, out int month, out int day) =>
            (year, month, day) = (date.Year, date.Month, date.Day);
    }
}
