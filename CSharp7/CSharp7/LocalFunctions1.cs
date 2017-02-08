// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;

namespace CSharp7
{
    class LocalFunctions1
    {
        static void Main()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(fib(i));
            }

            int fib(int n) => n < 2 ? n : fib(n - 1) + fib(n - 2);
        }

        static void NonLocalVariables()
        {
            // Works, but is ugly.
            Func<int, int> fib = null;
            fib = n => n < 2 ? n : fib(n - 1) + fib(n - 2);
        }
    }
}
