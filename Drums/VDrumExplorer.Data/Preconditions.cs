using System;

namespace VDrumExplorer.Data
{
    internal static class Preconditions
    {
        internal static T CheckNotNull<T>(T value, string paramName)
            where T : class =>
            value ?? throw new ArgumentNullException(paramName);
    }
}
