using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp7
{
    class TupleConstruction
    {
        static void Main()
        {
            var tuple1 = (1, 2);
            Console.WriteLine(tuple1.Item1);
            Console.WriteLine(tuple1.Item2);

            var tuple2 = new ValueTuple<int, long>(1, 2);
            Console.WriteLine(tuple2.Item1);
            Console.WriteLine(tuple2.Item2);

            var tuple4 = (a: 1, b: 2);
            Console.WriteLine(tuple4.a);
            Console.WriteLine(tuple4.b);
        }
    }
}
