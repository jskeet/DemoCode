// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class ScopedAs
    {
        static void Main()
        {
            ShowIfStringWithIs("foo");
        }

        static void ShowIfStringWithIs(object x)
        {
            if (x is string)
            {
                string text = (string)x;
                Console.WriteLine("It's a string! {0}", text);
            }
        }

        static void ShowIfStringWithAs(object x)
        {
            string text = x as string;
            if (text != null)
            {
                Console.WriteLine("It's a string! {0}", text);
            }
        }

        static void ShowIfStringWithEvil(object x)
        {
            for (string text = x as string; x != null; x = null)
            {
                Console.WriteLine("It's a string! {0}", text);
            }
        }
    }
}
