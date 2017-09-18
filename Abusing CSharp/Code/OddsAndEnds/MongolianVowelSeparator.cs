// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class MongolianVowelSeparator
    {
        static string stringx = "In initializer";

        static void Main()
        {
            string᠎x = "In Main";
            ShowField();
        }

        static void ShowField()
        {
            Console.WriteLine("stringx=" + stringx);
        }
    }
}



// The Mongolian Vowel Separator is U+180E
// History:
// 1996 - Unicode 2.0.0: Not present
// 1999 - 3.0.0: Cf
// 2014 - 7.0.0: Cf
// 2015 - 8.0.0: Cf
// 2016 - 9.0.0: Cf

// C#:
// MS C# 5 spec: 3.0
// ECMA 3rd ed: 4.0
// ECMA 4th ed + Roslyn: wave hands...