namespace Demos
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
            // This shouldn't.
            string doubledNullable = NullOrDouble(nullable);
        }

        // Null input leads to null output; non-null input leads to non-null output.
        // Reasonably common, but can't be expressed as far as I'm aware.
        static string? NullOrDouble(string? input) =>
            input == null ? null : input + input;    
    }
}
