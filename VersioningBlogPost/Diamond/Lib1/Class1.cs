using System;
using NodaTime;

namespace Lib1
{
    public class Class1
    {
        public static LocalDate GetFixedDate() =>
            new LocalDate(2019, 6, 29);

        public static Instant GetNow(IClock clock) =>
            clock.Now;
    }
}
