using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demos
{
    class NotExhaustive
    {
        static void Main()
        {
            int input = 5;
            int value = input switch
            {
                0 => 1,
                1 => 2,
                // Remove this to show the problem
                _ => 0
            };
            Console.WriteLine(value);
        }
    }
}
