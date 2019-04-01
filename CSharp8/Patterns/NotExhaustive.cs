using System;

namespace PatternsAndRanges
{
    class NotExhaustive
    {
        static void Main()
        {
            int input = 5;
            int value = input switch
            {
                0 => 1,
                1 => 2,
                _ => 0
            };

            Console.WriteLine(value);
        }
    }
}
