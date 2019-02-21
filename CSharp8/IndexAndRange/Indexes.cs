using System;

namespace IndexAndRange
{
    class Indexes
    {
        static void Main()
        {
            string text = "Conference";
            Index index = 1;
            Console.WriteLine(index);
            Console.WriteLine(text[index]);
            // Fails
            // index = -1;
            // Console.WriteLine(index);
            index = ^2;
            Console.WriteLine(index);
            Console.WriteLine(text[index]);
        }
    }
}
