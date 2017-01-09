using System;

namespace CSharp7
{
    class NamedTuple1
    {
        static void Main()
        {
            var anon = new { x = 5, y = 3, z = "hello" };
            var tuple = CreateTuple();
            tuple.x = 10;
            tuple = CreateTuple2();
            Console.WriteLine(tuple.x);
            Console.WriteLine(tuple.y);
            Console.WriteLine(tuple.z);
        }

        static (int x, int y, string z) CreateTuple()
        {
            return (5, 3, "hello");
        }

        static (int a, int b, string c) CreateTuple2()
        {
            return (10, 6, "world");
        }
    }
}
