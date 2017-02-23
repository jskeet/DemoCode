// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

#define FOO
using System;

namespace OddsAndEnds
{
    class PreprocessorOddities1
    {
        static void Main()
        {
#if FOO
            Console.WriteLine("FOO is defined");
            /* This is a comment
#else
            Console.WriteLine("FOO is not defined");
            /*/
#else
            /*/
            Console.WriteLine("FOO is still not defined");
#endif
        }
    }
}
