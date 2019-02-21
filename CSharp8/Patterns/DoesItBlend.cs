using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternsAndRanges
{
    class DoesItBlend
    {
        static void Main()
        {
            TryBlend(new object());
            TryBlend("this is a long string");
            TryBlend("short");
            TryBlend(1);
            TryBlend(2);
        }

        static void TryBlendWithoutSwitchExpressions(object value)
        {
            string message;
            switch (value)
            {
                case string x when x.Length > 10:
                    message = "Long strings jam up my blender";
                    break;
                case string x:
                    message = "Short strings blend";
                    break;
                case int x when (x & 1) == 0:
                    message = "Even numbers blend";
                    break;
                case int x:
                    message = "Odd numbers do not blend";
                    break;
                default:
                    message = $"{value} does not blend";
                    break;
            }
            Console.WriteLine(message);
        }

        static string TryBlend(object value) =>
            value switch
            {
                5 => "",
                string x when x.Length > 10 =>
                    "Long strings jam up my blender",
                string x => "Short strings blend",
                int x when (x & 1) == 0 => "Even numbers blend",
                int x => "Odd numbers do not blend",
                _ => $"{value} does not blend"
            };        
    }
}
