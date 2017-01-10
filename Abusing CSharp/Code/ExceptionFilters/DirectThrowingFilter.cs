// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace ExceptionFilters
{
    class DirectThrowingFilter
    {
        static void Main()
        {
            try
            {
                Foo();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
        }

        static void Foo()
        {
            try
            {
                Bar();
            }
            catch (Exception e) when (e.Message[10] == '!')
            {
            }
        }

        static void Bar()
        {
            throw new Exception("Bang!");
        }
    }
}
