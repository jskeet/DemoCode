using System;
using System.Diagnostics.CodeAnalysis;

namespace Nullability
{
    class Precondition
    {
        static void Main()
        {
            string? text = MaybeNull();
            string textNotNull = CheckNotNull(text);
            // This is relatively obvious...
            Console.WriteLine(textNotNull.Length);

            // This is less so. The NotNull attribute implies that the
            // method will be validating the input, so the method
            // won't return if the value is null.
            // It's possible this isn't actually intentional/desirable.
            // See https://github.com/dotnet/roslyn/issues/37544
            Console.WriteLine(text.Length);
        }

        static T CheckNotNull<T>([NotNull] T? input) where T : class =>
            input ?? throw new ArgumentNullException();

        static string? MaybeNull() => DateTime.UtcNow.Second == 0 ? null : "not null";
    }
}
