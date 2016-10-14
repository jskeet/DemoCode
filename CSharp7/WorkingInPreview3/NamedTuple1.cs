using System;

namespace WorkingInPreview3
{
    class NamedTuple1
    {
        static void Main()
        {
            var tuple = CreateTuple();
            Console.WriteLine(tuple.x);
            Console.WriteLine(tuple.y);
            Console.WriteLine(tuple.z);
        }

        static (int x, int y, string z) CreateTuple()
        {
            return (5, 3, "hello");
        }
    }
}
