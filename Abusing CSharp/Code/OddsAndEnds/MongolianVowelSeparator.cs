// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class MongolianVowelSeparator
    {
        static string stringx = "In initializer"; 

        static void Main()
        {
            // The Mongolian Vowel Separator is U+180E
            // History:
            // 1996 - Unicode 2.0.0: Not present
            // 1999 - 3.0.0: Cf
            // 2003 - 4.0.0: Zs
            // 2006 - 5.0.0: Zs
            // 2010 - 6.0.0: Zs
            // 2014 - 7.0.0: Cf
            // 2015 - 8.0.0: Cf
            // 2016 - 9.0.0: Cf
            Console.WriteLine(char.GetUnicodeCategory('\u180e'));
            string᠎x = "In Main";

            ShowField();
        }

        static void ShowField()
        {
            Console.WriteLine("stringx=" + stringx);
        }
    }
}
