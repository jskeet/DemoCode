using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToOperators
{
    class Power
    {
        // As suggested by Nat Pryce at NorDevCon, 2014-02-28
        static void Main()
        {
            var source = new[] { "foo", "bar", "baz" }.Evil();

            var cubed = source ^ 3;
            Console.WriteLine(cubed);
        }
    }
}
