// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class ThrowExpression
    {
        static void Main()
        {
            string foo = null;
            string bar = CheckNotNull(foo, nameof(foo));
        }

        static T CheckNotNull<T>(T value, string name) where T : class
            => value ?? throw new ArgumentNullException(name);
    }
}
