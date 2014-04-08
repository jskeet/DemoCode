// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections.Generic;

namespace LinqToOperators
{
    public sealed class DarkEnumerable
    {
        private readonly IEnumerable<object> source;

        internal DarkEnumerable(IEnumerable<object> source)
        {
            this.source = source;
        }

        internal IEnumerable<dynamic> Source { get { return source; } }

        public static OperatorEnumerable operator -(DarkEnumerable operand)
        {
            return new OperatorEnumerable(operand.source);
        }
    }
}
