using System;

namespace WorkingInPreview3
{
    class TryParseRevisited
    {
        static void Main()
        {
            Console.WriteLine(TryParse("5"));
            Console.WriteLine(TryParse("Whoops"));

            var parseResult = TryParse("5");
            if (parseResult.success)
            {
                Console.WriteLine(parseResult.value);
            }
        }

        static (bool success, int value) TryParse(string x)
        {
            int value;
            // Somewhat confusing implementation, I'll admit...
            return (int.TryParse(x, out value), value);
        }
    }
}
