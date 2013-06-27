// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections;

namespace LinqToOperators
{
    public static class Extensions
    {
        public static OperatorEnumerable Evil(this IEnumerable source)
        {
            return new OperatorEnumerable(source);
        }
    }
}
