// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace ProductionCode.Utilities
{
    internal static class Preconditions
    {
        public static T CheckNotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
            return value;
        }
    }
}
