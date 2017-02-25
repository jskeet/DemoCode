using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddsAndEnds
{
    class DelegateEquality
    {
        static void Main()
        {
            Action a1 = Main;
            Action a2 = Main;

            MulticastDelegate md1 = a1;
            MulticastDelegate md2 = a2;

            Delegate d1 = a1;
            Delegate d2 = a2;

            object o1 = a1;
            object o2 = a2;
            
            Console.WriteLine(a1 == a2);
            Console.WriteLine(md1 == md2);
            Console.WriteLine(d1 == d2);
            Console.WriteLine(o1 == o2);
        }
    }
}
