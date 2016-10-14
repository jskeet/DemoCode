using NodaTime;
using NodaTime.Text;

namespace NotInPreview3
{
    class PropertyMatch
    {
        public static object LocalDate { get; private set; }

        static void Main()
        {
            ParseResult<LocalDate> parseResult = LocalDatePattern.IsoPattern.Parse("2016-08-05");
            if (parseResult is ParseResult<LocalDate> { Success is true, Value is var value })
            {
                Console.WriteLine($"Parsed value: value is {value}");
            }
        }
    }
}
