// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System.Xml.Linq;

namespace CSharp7
{
    class TupleConversion
    {
        static void Main()
        {
            // Predefined conversion from int to long
            (long, long) integers = (10, 2);

            // Predefined conversions from constant expressions
            // in range of byte
            (byte, byte) bytes = (5, 10);

            // Element-wise predefined conversion
            (double, double) doubles = integers;

            object boxed = integers;
            // This will throw InvalidCastException
            // doubles = ((double, double)) boxed;

            // Element-wise user-defined conversions
            (string, string) names = ("http://...", "http://...");
            (XNamespace, XNamespace) namespaces = names;
        }
    }
}
