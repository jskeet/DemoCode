using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DigiMixer.Diagnostics;

/// <summary>
/// Extension methods to make working with null values (and nullable reference types) simpler.
/// </summary>
public static class NullExtensions
{
    public static T OrThrow<T>([NotNull] this T? text, [CallerArgumentExpression(nameof(text))] string? message = null) where T : class =>
        text ?? throw new InvalidDataException($"No value for '{message}'");

    public static T OrThrow<T>([NotNull] this T? text, [CallerArgumentExpression(nameof(text))] string? message = null) where T : struct =>
        text ?? throw new InvalidDataException($"No value for '{message}'");
}
