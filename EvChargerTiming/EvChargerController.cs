// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.using NodaTime;

using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;

namespace EvChargerTiming;

public class EvChargerController
{
    // Note: Noda Time exposes a ZonedClock which could be used for this,
    // which just composes a clock and a zone, but I've separated them out for
    // clarity.
    private readonly DateTimeZone zone;
    private readonly IClock clock;
    private readonly ChargingSchedule schedule;
    private readonly EvCharger charger;
    private readonly ILogger logger;

    public EvChargerController(EvCharger charger, ChargingSchedule schedule, DateTimeZone zone, IClock clock, ILogger logger)
    {
        this.charger = charger;
        this.schedule = schedule;
        this.zone = zone;
        this.clock = clock;
        this.logger = logger;
    }

    /// <summary>
    /// Infinite loop which just checks periodically whether the charger should be on or not,
    /// based on the schedule.
    /// </summary>
    /// <param name="pollingInterval">The interval at which to check whether or not the charger should be on.</param>
    public void MainLoop(TimeSpan pollingInterval)
    {
        // In a real system we'd want ways of shutting down, updating the schedule,
        // changing the system time zone, updating the time zone database etc.
        while (true)
        {
            Instant now = clock.GetCurrentInstant();

            // Note: converting an instant into a local date/time is always unambiguous. (Every
            // instant maps to exactly one local date/time.) Conversions in the opposite
            // direction may be ambiguous or invalid.
            ZonedDateTime nowInTimeZone = now.InZone(zone);

            bool shouldBeOn = schedule.IsChargingEnabled(nowInTimeZone.LocalDateTime);
            if (charger.On != shouldBeOn)
            {
                logger.LogInformation("At {now} ({local} local), changing state to {state}",
                    InstantPattern.ExtendedIso.Format(now),
                    ZonedDateTimePattern.GeneralFormatOnlyIso.Format(nowInTimeZone),
                    shouldBeOn);

                charger.ChangeState(shouldBeOn);
            }

            // We *could* predict when we'll next need to turn the charger on.
            // However, that's significantly more fiddly than just checking periodically.
            // A check of something "once per minute" will take very little power,
            // and be much simpler than trying to predict how long to sleep for.
            // The reality of EV charing 
            Thread.Sleep(pollingInterval);
        }
    }
}
