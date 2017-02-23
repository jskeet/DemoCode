// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    using static OddsAndEnds.DateFormatting.IsoDateFormat;

    class DateDeconstruction2
    {
        static void Main()
        {
            DateTime today = DateTime.Today;
            var (x, y, z) = today;
            Console.WriteLine($"{x}/{y}/{z}");
        }
    }
}
