using System.Diagnostics.CodeAnalysis;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    internal static class Validation
    {
        // TODO: We want an attribute that says "If this method returns normally, value wasn't null."
        // See https://github.com/dotnet/corefx/issues/37826

        internal static T ValidateNotNull<T>(FieldPath path, T? value, string name) where T : class =>
            value ?? throw new ModuleSchemaException(path, $"{name} must be specified");

        internal static T ValidateNotNull<T>(FieldPath path, T? value, string name) where T : struct =>
            value ?? throw new ModuleSchemaException(path, $"{name} must be specified");

        internal static void ValidateNull<T>(FieldPath path, T? value, string name, string becauseOfName)
            where T : class
        {
            if (value is T)
            {
                throw new ModuleSchemaException(path, $"{name} must not be specified because of {becauseOfName}");
            }
        }

        internal static void Validate(FieldPath path, bool condition, string message)
        {
            if (!condition)
            {
                throw new ModuleSchemaException(path, message);
            }
        }
        
        internal static T ValidateNotNull<T>(T? value, string name) where T : class =>
            value ?? throw new ModuleSchemaException($"{name} must be specified");

        internal static T ValidateNotNull<T>(T? value, string name) where T : struct =>
            value ?? throw new ModuleSchemaException($"{name} must be specified");

        internal static void Validate([DoesNotReturnIf(false)] bool condition, string message)
        {
            if (!condition)
            {
                throw new ModuleSchemaException(message);
            }
        }
    }
}
