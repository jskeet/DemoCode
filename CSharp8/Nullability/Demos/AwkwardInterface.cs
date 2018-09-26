using System;
using System.Collections.Generic;

namespace Demos
{
    // Should this be string or string? ?
    class AwkwardInterface : IEqualityComparer<string?>
    {
        static void Main()
        {
            var comparer = new AwkwardInterface();
            // This should work
            comparer.Equals(null, null);
            // This should warn - it's going to throw
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

        public int GetHashCode(string? obj)
        {
            // That's a bit weird...
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
            return obj.Length;
        }
    }
}
