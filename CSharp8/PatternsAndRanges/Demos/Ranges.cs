using System;

namespace Demos
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
            // Note: should be able to use span[6..] but it doesn't
            // compile, despite the extra extension method :(
            ReadOnlySpan<char> spanSlice = span.Slice(6..);
            Console.WriteLine(spanSlice.ToString());
        }
    }
}
