using System;

namespace MinorFeatures
{
    class NullCoalescingAssignment
    {
        static void Main()
        {
            string x = null;

            // Before...
            x = x ?? "foo";
            // With C# 8
            x ??= "bar";

            Console.WriteLine(x);
        }
    }
}
