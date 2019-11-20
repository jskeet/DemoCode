#nullable disable warnings
#nullable enable annotations

using System;

namespace Nullability
{
    class AnnotationsOnlyContext
    {
        static void Main()
        {
            string? x = null;
            string y = null;
            Console.WriteLine(x.Length);
            Console.WriteLine(y.Length);
        }
    }
}
