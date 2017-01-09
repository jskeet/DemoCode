// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

#define FOO
using System;
using System.ComponentModel;

namespace OddsAndEnds
{
    class PreprocessorOddities2
    {
        static void Main()
        {
#if FOO
            string x = @"FOO is defined
#else
            string x = @";
#else
                         FOO is not defined";
#endif
            Console.WriteLine(x);
        }
    }
}
