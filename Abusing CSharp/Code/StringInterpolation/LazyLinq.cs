using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringInterpolation
{
    class LazyLinq
    {
        static void Main()
        {
            int[] values = { 1, 4, 1, 2, 5, 6 };
            int min = 3;
            var query = from v in values where v >= min select v;
            // Equivalent to: var query = values.Where(v => v >= min);

            Console.WriteLine(query.Count());
            min = 2;
            Console.WriteLine(query.Count());
        }
    }
}
