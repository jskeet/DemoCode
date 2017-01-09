using System;

namespace CSharp7
{
    class OutVar
    {
        static void Main()
        {
            // Explicit typing, but still introducing variable in if
            if (int.TryParse("5", out int value))
            {
                Console.WriteLine(value);
            }            

            // var as well...
            if (int.TryParse("5", out var value2))
            {
                Console.WriteLine(value);
            }
        }
    }
}
