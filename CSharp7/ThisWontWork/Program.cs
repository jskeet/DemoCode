using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThisWontWork
{
    class UnnamedTuple
    {
        static void Main()
        {
            (int, int, string) tuple = CreateTuple();
            Console.WriteLine(tuple.Item1);
            Console.WriteLine(tuple.Item2);
            Console.WriteLine(tuple.Item3);
        }

        static (int, int, string) CreateTuple()
        {
            return ValueTuple.Create(5, 3, "hello");
        }
    }
}
