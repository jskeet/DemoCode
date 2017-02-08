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

        // Could use switch/case with pattern matching now
        static int DoSomething(string x) =>
            x == "foo" ? 10
            : x == "bar" ? 20
            : x.Length > 5 ? 30
            : throw new ArgumentException("x must be foo or bar, or longer than 5");
    }
}
