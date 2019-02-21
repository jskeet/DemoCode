using System;

namespace IndexAndRange
{
    class Ranges
    {
        static void Main()
        {
            // Simple range within a string
            string text = "Hello world";
            Range range = 1..^2;
            string stringSlice = text[range];
            Console.WriteLine(stringSlice);

            // Looks backwards...
            stringSlice = text[^8..5];
            Console.WriteLine(stringSlice);

            ReadOnlySpan<char> span = text.AsSpan();
            ReadOnlySpan<char> spanSlice = span[6..];
            Console.WriteLine(spanSlice.ToString());
        }
    }
}
