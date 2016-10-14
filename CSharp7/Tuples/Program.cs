using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tuples
{
    class Program
    {
        static void Main(string[] args)
        {
            var(sum, count) = SumAndCount(1, 5, 10);
        }

        static (int sum, int count) SumAndCount(params int[] values)
        {

        }
    }
}
