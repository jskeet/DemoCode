using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nullability
{
    // Should this be string or string? ?
    class AwkwardInterface : IEqualityComparer<string?>
    {
        static void Main()
        {
            var comparer = new AwkwardInterface();
            // This should work
            comparer.Equals(null, null);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            comparer.GetHashCode(null);
        }

        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }
            // Bizarre, but hey...
            return x.Length == y.Length;
        }

        // We're forced to use string? due to the interface, but we don't want to actually allow null.
        public int GetHashCode([DisallowNull] string? obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
            return obj.Length;
        }
    }
}
