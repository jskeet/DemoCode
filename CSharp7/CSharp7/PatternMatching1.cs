using System;

namespace CSharp7
{
    class PatternMatching1
    {
        static void Main()
        {
            PrintInt32s("foo");
            PrintInt32s(10);

            PointlessConstantMatch(10);
            PointlessConstantMatch(5);
            
            LessPointlessConstantMatch(10);
            LessPointlessConstantMatch(5);
            LessPointlessConstantMatch(5L);
            LessPointlessConstantMatch("foo");          
        }
        
        static void PrintInt32s(object x)
        {
            if (x is int i)
            {
                Console.WriteLine($"Got an Int32: {i}");
            }            
        }

        static void PointlessConstantMatch(int x)
        {
            if (x is 5)
            {
                Console.WriteLine("Yes, x is 5");
            }
        }

        static void LessPointlessConstantMatch(object x)
        {
            if (x is 5)
            {
                Console.WriteLine("Yes, x is 5");
            }
        }
    }
}
