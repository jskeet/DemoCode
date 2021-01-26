// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscCore;
using System;
using System.Linq;

namespace OscMixerControl.Wpf
{
    internal static class LoggerExtensions
    {
        internal static void LogPacket(this ILogger logger, OscPacket packet)
        {
            if (packet is OscMessage message && message.Address.StartsWith("/meters"))
            {
                LogMeterMessage(logger, message);
            }
            else
            {
                logger.LogInformation("Received {packet}", packet.ToString());
            }
        }

        private static void LogMeterMessage(ILogger logger, OscMessage message)
        {
            var blob = (byte[])message[0];
            int valueCount = BitConverter.ToInt32(blob, 0);

            var values = Enumerable.Range(0, valueCount)
                .Select(index => BitConverter.ToInt16(blob, index * 2 + 4))
                .ToList();

            logger.LogInformation("Received meter {meter} ({count}): {values}", message.Address, valueCount, string.Join(" ", values.Select(v => v.ToString("x4"))));
        }
    }
}
