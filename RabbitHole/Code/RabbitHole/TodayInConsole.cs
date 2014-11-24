// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Globalization;

namespace RabbitHole
{
    class TodayInConsole
    {
        static void Main()
        {
            var culture = new CultureInfo("he-IL");
            var dtfi = culture.DateTimeFormat;
            dtfi.Calendar = new HebrewCalendar();
            var today = DateTime.Today;
            string text = today.ToString("yyyy MMM dd", dtfi);
            Console.WriteLine(text);
        }
    }
}
