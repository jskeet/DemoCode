using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp7
{
    class TupleParameters
    {
        static void Main()
        {
            (int foo, int bar) result = Add((5, 3), (6, 4));
            (int asd, int fgh) other = (5, 3);
            Console.WriteLine(result.foo);
            Console.WriteLine(result.bar);
            Console.WriteLine(result);
        }

        static (int a, int b) Add((int a, int b) x, (int, int) y)
        {
            return (x.a + y.Item1, x.b + y.Item2);
        }
    }
}
