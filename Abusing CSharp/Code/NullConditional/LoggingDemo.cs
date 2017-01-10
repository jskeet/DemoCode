// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace NullConditional
{
    class LoggingDemo
    {
        static void Main()
        {
            int x = 10;
            Logger logger = new Logger();
            logger.Debug?.Log($"Debug log entry");
            logger.Info?.Log($"Info at {DateTime.UtcNow}; x={x}");
        }
    }
}
