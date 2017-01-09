// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace WorkingInPreview3
{
    class TryParseRevisited
    {
        static void Main()
        {
            Console.WriteLine(TryParse("5"));
            Console.WriteLine(TryParse("Whoops"));

            var parseResult = TryParse("5");
            if (parseResult.success)
            {
                Console.WriteLine(parseResult.value);
            }
        }

        static (bool success, int value) TryParse(string x)
        {
            // Somewhat confusing implementation, I'll admit...
            return (int.TryParse(x, out int value), value);
        }
    }
}
