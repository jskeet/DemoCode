// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace ExceptionFilters
{
    class ReflectionThrowingFilter
    {
        static void Main()
        {
            Foo();
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
