// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using ProductionCode.Attributes;
using ProductionCode.Utilities;

namespace ProductionCode
{
    public class SampleCode
    {
        public static void Foo([NotNull] string a, [NotNull] string b, [NotNull] string c, string d)
        {
            Preconditions.CheckNotNull(b, "b");
            Preconditions.CheckNotNull("c", c);
            Preconditions.CheckNotNull(d, "e");
            DoSomethingWith(a);
            Console.WriteLine(a.Length);
        }

        private static void DoSomethingWith([NotNull] string a)
        {
            Preconditions.CheckNotNull(a, "a");
        }
    }
}
