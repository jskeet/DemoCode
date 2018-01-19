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
            DoSomething("");
        }

        static T CheckNotNull<T>(T value, string name) where T : class
            => value ?? throw new ArgumentNullException(name);

        // Could use switch/case with pattern matching now.
        // Wouldn't it be nice to have an expression-bodied switch/case?
        static int DoSomething(string x) =>
            x == "foo" ? 10
            : x == "bar" ? 20
            : x.Length > 5 ? 30
            : throw new ArgumentException("x must be foo or bar, or longer than 5");
    }
}
