using System;

namespace CSharp7
{
    class TupleMembers
    {
        static void Main()
        {
            var t1 = (x: 1, y: 2);
            var t2 = (a: 1, b: 2);
            var t3 = (a: 1, b: 2);
            Console.WriteLine(t1.ToString()); // (1, 2)
            Console.WriteLine(t1.Equals(t2)); // True
            // Doesn't compile! Console.WriteLine(t1 == t3); // True
            // Doesn't compile! Console.WriteLine(t2 == t3); // True
        }
    }
}
