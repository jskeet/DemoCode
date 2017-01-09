using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullConditional
{
    class LazyDetailedExceptions
    {
        static void Main()
        {
            Foo(3, 5);
            Foo(5, 4);
        }

        static void Foo(int start, int end)
        {
            Preconditions.CheckArgument(start <= end, nameof(start))?.Report($"Start must be <= end; start={start}; end={end}");
        }
    }
}
