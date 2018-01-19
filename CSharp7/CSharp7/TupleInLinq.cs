// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections.Generic;
using System.Linq;

namespace CSharp7
{
    class TupleInLinq
    {
        static void Main()
        {
            // See http://stackoverflow.com/questions/39386251
            var list = new List<string> { "a", "b" };
            // In C# 7.1 the element name would be inferred: (value, index) => (value, index)
            var dictionary = list.Select((value, index) => (value: value, index: index))
                                 .ToDictionary(pair => pair.index, pair => pair.value);
        }
    }
}
