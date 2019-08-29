using System;

namespace MinorFeatures
{
    class VerbatimInterpolatedStringLiterals
    {
        static void Main()
        {
            // Only of these was valid in C# 7. I can't remember which.
            Console.WriteLine(@$"This is valid");
            Console.WriteLine($@"This is valid too");
        }
    }
}
