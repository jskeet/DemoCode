// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class NumericLiterals
    {
        static void Main()
        {
            int alternateBits = 0b10101010;
            int argb = 0x7f_ff_00_77;
            int million = 1_000_000;

            Console.WriteLine(alternateBits);
            Console.WriteLine(argb);
            Console.WriteLine(million);
        }
    }
}
