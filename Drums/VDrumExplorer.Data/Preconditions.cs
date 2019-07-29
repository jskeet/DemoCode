// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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
