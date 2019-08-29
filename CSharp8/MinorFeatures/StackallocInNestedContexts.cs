using System;

namespace MinorFeatures
{
    class StackallocInNestedContexts
    {
        static void Main()
        {
            // Fine before C# 8
            Span<int> span = stackalloc int[5];
            M(span);

            // Only valid in C# 8
            M(stackalloc int[10]);
        }

        static void M(Span<int> span)
        {
        }
    }
}
