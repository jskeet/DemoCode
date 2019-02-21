using System;

namespace Nullability
{
    class ConditionalReturn
    {
        static void Main()
        {
            string? text = null;
            // string.IsNullOrEmpty will definitely return true for null
            // input. It will return false for *some* but not all non-null 
            // input. How can the compiler know what this means?
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("It was null or empty");
            }
            else
            {
                // This should be okay - we know text isn't null
                // at this point.
                Console.WriteLine(text.Length);
            }
        }
    }
}
