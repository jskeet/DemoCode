// Hmm... no need for this from the command line, but Visual Studio is a bit behind.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

using System.Diagnostics.CodeAnalysis;

namespace Nullability
{
    class NullPropagation
    {
        static void Main()
        {
            string text = "foo";
            // This should be okay - we know the input isn't null, so
            // we should be able to know the output isn't null.
            string doubled = NullOrDouble(text);

            string? nullable = null;
            // This should (and does) give a warning
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string doubledNullable = NullOrDouble(nullable);
        }

        // Null input leads to null output; non-null input leads to non-null output.
        // Side issue: it would be nice to be able to use nameof here.
        [return: NotNullIfNotNull("input")]
        static string? NullOrDouble(string? input) =>
            input == null ? null : input + input;    
    }
}
