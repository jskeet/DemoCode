using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp7
{
    public static class ValueTupleExtensions
    {
        public static ValueTuple<T1, T2, T3> With<T1, T2, T3>(
            this ValueTuple<T1, T2> tuple, T3 other)
            => (tuple.Item1, tuple.Item2, other);
    }

    class TupleTypeEquivalence
    {
        static void Main()
        {
            var t1 = (1, 1);
            var t2 = (x: 1, y: 2);
            var t3 = (a: 2, b: 1);
            Console.WriteLine(t1.GetType() == t1.GetType()); // True
            Console.WriteLine(t1.GetType() == t3.GetType()); // True
            t3 = t2;
            Console.WriteLine(t3.a); // 1

            var t4 = (a: 1, b: 2L); // Note long second property
            Console.WriteLine(t1.GetType() == t4.GetType()); // False

            var arity3 = (a: 1, b: 2, c: 3);
            arity3 = t1.With(2);

            // t4 = t3; // Fails to compile, despite component-wise implicit conversions
        }
    }
}
