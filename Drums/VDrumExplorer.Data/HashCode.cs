// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#if NET472

// Just a simple (but inefficient) implementation of the System.HashCode.Combine method.
// It's possible that it's in a NuGet package somewhere, but let's just add it...
namespace VDrumExplorer.Data
{
    public static class HashCode
    {
        public static int Combine(params object[] values)
        {
            unchecked
            {
                int hash = 19;
                foreach (var value in values)
                {
                    hash = hash * 31 + value?.GetHashCode() ?? 0;
                }
                return hash;
            }
        }
    }
}
#endif