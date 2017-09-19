using NodaTime.Calendars;
using NUnit.Framework;

namespace NodaTime.Test.Calendars
{
    public class UmAlQuraYearMonthDayCalculatorTest
    {
        [Test]
        public void GetYearMonthDay_DaysSinceEpoch()
        {
            var calculator = new UmAlQuraYearMonthDayCalculator();
            int daysSinceEpoch = calculator.GetStartOfYearInDays(calculator.MinYear);
            for (int year = calculator.MinYear; year <= calculator.MaxYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    for (int day = 1; day <= calculator.GetDaysInMonth(year, month); day++)
                    {
                        var actual = calculator.GetYearMonthDay(daysSinceEpoch);
                        var expected = new YearMonthDay(year, month, day);
                        Assert.AreEqual(expected, actual, "daysSinceEpoch={0}", daysSinceEpoch);
                        daysSinceEpoch++;
                    }
                }
            }
        }
    }
}