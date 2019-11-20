#nullable enable warnings
#nullable disable annotations

using System;

namespace Nullability
{
    class WarningsOnlyContext
    {
        static void Main()
        {
            Check(null);
            Check("not null");
        }

        static void Check(string x)
        {
            if (x is null)
            {
                Console.WriteLine("x is null");
            }
            else
            {
                Console.WriteLine($"x is not-null, with length {x.Length}");
            }
        }
    }
}
