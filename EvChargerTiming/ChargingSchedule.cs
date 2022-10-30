// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime;

namespace EvChargerTiming;

/// <summary>
/// The rules for a single day within a charging schedule. 
/// <see cref="Start"/> is inclusive; <see cref="End"/> is exclusive, and we assume
/// that Start is less than or equal to End (validated elsewhere). In a real system
/// we'd want to be able to handle "charge to the end of the day" (and there's no LocalTime
/// representation of "midnight at the end of the day"), and potentially be able to have
/// "End less than Start" options for charging between (say) 11pm and 2am.
/// 
/// All of that logic could be added here, and it wouldn't affect the design in terms of
/// time zone conversions (which is the point of this sample code).
/// </summary>
public record ChargingScheduleDay(IsoDayOfWeek DayOfWeek, LocalTime Start, LocalTime End)
{
    public bool Contains(LocalTime now) =>
        Start <= now && now < End;
}

/// <summary>
/// A schedule for the EV charger, expressed as one <see cref="ChargingScheduleDay"/> per day of the week.
/// </summary>
public class ChargingSchedule
{
    private readonly List<ChargingScheduleDay> days;

    public ChargingSchedule(List<ChargingScheduleDay> days)
    {
        // TODO: Validation that there's exactly one entry per day-of-the-week, etc.
        this.days = days;
    }

    /// <summary>
    /// Given the local date and time, should the charger be on or off?
    /// </summary>
    public bool IsChargingEnabled(LocalDateTime dateTime)
    {
        var day = days.Single(candidate => candidate.DayOfWeek == dateTime.DayOfWeek);
        return day.Contains(dateTime.TimeOfDay);
    }
}
