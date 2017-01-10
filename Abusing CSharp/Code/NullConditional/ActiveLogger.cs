// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Globalization;

namespace NullConditional
{
    class ActiveLogger
    {
        private static readonly CultureInfo LoggingCulture = GetLoggingCulture();
        private readonly string level;

        public ActiveLogger(string level)
        {
            this.level = level;
        }

        public void Log(FormattableString message)
        {
            Console.WriteLine($"{level}: {message.ToString(LoggingCulture)}");
        }

        private static CultureInfo GetLoggingCulture()
        {
            var culture = (CultureInfo) CultureInfo.InvariantCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            return culture;
        }
    }
}
