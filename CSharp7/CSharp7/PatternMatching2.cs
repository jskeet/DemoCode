using System;

namespace CSharp7
{
    class PatternMatching2
    {
        static void Main()
        {
            Console.WriteLine(DoesItBlend(1L));
            Console.WriteLine(DoesItBlend("short"));
            Console.WriteLine(DoesItBlend("Long text"));
            Console.WriteLine(DoesItBlend(5));
            Console.WriteLine(DoesItBlend(6));
            Console.WriteLine(DoesItBlend(7));
        }

        static string DoesItBlend(object x)
        {
            switch (x)
            {
                case 5: return "5s never blend";
                case int y when (y & 1) == 1: return "Odd numbers other than 5 always blend";
                // Scoping of y is interesting here. This may change.
                case int y: return "Even numbers are unreliable blenders";
                case string name when name.Length > 5: return "Long text blends";
                case var o: return $"Values of type {o.GetType()} may blend, or may not. I don't know.";
            }
        }
    }
}
