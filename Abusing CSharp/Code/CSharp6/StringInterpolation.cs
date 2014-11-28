using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp6
{
    class StringInterpolation
    {
        static void Main()
        {
            // Braces for multiple statements aren't allowed...
            // Console.WriteLine("\{{Console.WriteLine("hello"); return "y";}}");

            // But lambda expressions are!
            Console.WriteLine("\{((Func<string>)(() => {Console.WriteLine("hello"); return "y";}))()}");
            // Simplified with a method call to avoid the cast...
            Console.WriteLine("\{F(() => { Console.WriteLine("hello"); return "y"; })}");
        }

        static string F(Func<string> func)
        {
            return func();
        }
    }
}
