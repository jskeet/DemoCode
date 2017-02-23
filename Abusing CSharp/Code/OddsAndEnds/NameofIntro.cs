using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddsAndEnds
{
    class NameofIntro
    {
        static void HelloWorld()
        {
        }

        static void Main()
        {
            Console.WriteLine(nameof(HelloWorld));
            string x = null;
            Console.WriteLine(nameof(x.IsNullOrEmpty));
        }
    }
}
