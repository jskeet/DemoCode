using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
{
    class TupleConstruction
    {
        static void Main()
        {
            var tuple1 = (1, 2);
            Console.WriteLine(tuple1.Item1);
            Console.WriteLine(tuple1.Item2);

            var tuple2 = new(int, long)(1, 2);
            Console.WriteLine(tuple2.Item1);
            Console.WriteLine(tuple2.Item2);

            var tuple3 = new(int x, int y) { x = 1, y = 2 };
            Console.WriteLine("Read this line: " + tuple3.x);
            Console.WriteLine(tuple3.y);

            var tuple4 = (a: 1, b: 2);
            Console.WriteLine(tuple4.a);
            Console.WriteLine(tuple4.b);
        }
    }
}
