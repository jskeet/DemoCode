using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
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

namespace WorkingInPreview3
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
