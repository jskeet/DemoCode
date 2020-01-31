using System;
using System.Diagnostics.CodeAnalysis;

namespace Nullability
{
    class ConditionalReturn
    {
        static void Main()
        {
            // Infer nullability of an argument based on the return value.
            string? text = null;
            if (StringIsNullOrEmpty(text))
            {
                Console.WriteLine("It was null or empty");
            }
            else
            {
                // This should be okay - we know text isn't null
                // at this point.
                Console.WriteLine(text.Length);
            }

            // Type system says value will be not-null.
            // Attribute says it might actually be null.
            // Interestingly, can't use "out string value" here
            // even though that's the inferred type!
            if (!TryGetValue(false, out var value))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Console.WriteLine(value.Length);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        static bool StringIsNullOrEmpty([NotNullWhen(false)] string? text) =>
            string.IsNullOrEmpty(text);

        // Imagine a Dictionary<bool, string>
        static bool TryGetValue(bool key, [MaybeNullWhen(false)] out string value)
        {
            if (key)
            {
                value = "Found the key";
                return true;
            }
            else
            {
                value = null!;
                return false;
            }
        }
    }
}
