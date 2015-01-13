// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Globalization;
using System.Linq;

namespace RabbitHole
{
    class ShowMonthsEnglish
    {
        static void Main()
        {
            var dtfi = new CultureInfo("he-IL").DateTimeFormat;
            var calendar = new HebrewCalendar();
            dtfi.Calendar = calendar;
            dtfi.MonthNames = new[] {
                "Tishri", "Heshvan", "Kislev", "Tevet",
                "Shevat", "Adar", "Adar II", "Nisan", "Iyar", "Sivan", "Tamuz",
                "Av", "Elul"
            };
            var lines = from year in Enumerable.Range(5775, 2)
                        from month in Enumerable.Range(1, calendar.GetMonthsInYear(year))
                        select string.Format(dtfi,
                            "{0}-{1}: {2:MMMM}", year, month, new DateTime(year, month, 1, calendar));
            FormHelper.ShowText(lines);
        }
    }
}
