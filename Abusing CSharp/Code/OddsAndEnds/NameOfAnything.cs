// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class NameOfAnything
    {
        static void Main()
        {
            dynamic d = null;
            Console.WriteLine(nameof(d.Anything));
            Console.WriteLine(nameof(d.Mongolian᠎Vowel᠎Separator));
            Console.WriteLine(nameof(d.Mongolian᠎Vowel᠎Separator).Length);
            Console.WriteLine("Mongolian᠎Vowel᠎Separator".Length);
        }
    }
}
