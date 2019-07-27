using System;
using System.Diagnostics.CodeAnalysis;

namespace VDrumExplorer.Data
{
    internal static class Preconditions
    {
        internal static T CheckNotNull<T>(T value, string paramName)
            where T : class =>
            value ?? throw new ArgumentNullException(paramName);
    }
}
