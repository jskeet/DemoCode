// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    using static OddsAndEnds.DateFormatting.DateTimeExtensions;

    class DateDeconstruction
    {
        static void Main()
        {
            DateTime today = DateTime.Today;
            var (x, y, z) = today;
            Console.WriteLine(today);
        }
    }
}
