using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demos
{
    class DoesItBlend
    {
        static void Main()
        {
            TryBlend("this is a long string");
            TryBlend("short");
            TryBlend(1);
            TryBlend(2);
        }

        static void TryBlend(object value)
        {
            string message = value switch
            {
                string x when x.Length > 10 =>
                    "Long strings jam up my blender",
                string x => "Short strings blend",
                int x when (x & 1) == 0 => "Even numbers blend",
                int x => "Odd numbers do not blend",
                _ => $"{value} does not blend"
            };
            Console.WriteLine(message);
        }
    }
}
