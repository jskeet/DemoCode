#pragma warning disable CS8602
using System;

namespace Nullability
{
    class ReferenceEquality
    {
        static void Main()
        {
            string? text = null;

            if (ReferenceEquals(text, null))
            {
                Console.WriteLine("text was null");
            }
            else
            {
                // If we're calling object.ReferenceEquals, one operand is a constant
                // null, and the return value is false, we should know the other operand
                // was not null.
                Console.WriteLine(text.Length);
            }
        }
    }
}
