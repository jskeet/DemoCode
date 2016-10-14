using System;

namespace NotInPreview3
{
    class ExplicitTupleDecomposition
    {
        static void Main()
        {
            var (a, b, c) = CreateTuple();
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
        }

        static (int x, int y, string z) CreateTuple()
        {
            return (5, 3, "hello");
        }
    }
}
