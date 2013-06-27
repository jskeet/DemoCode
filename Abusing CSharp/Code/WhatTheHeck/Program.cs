// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace WhatTheHeck
{
    class Program
    {
        static void Main(string[] args)
        {
            var limit = 10;
            limit = "five";
            for (var x = 0; x < limit; x++)
            {
                Console.WriteLine(x);
                Console.WriteLine("The current value of x is {0}: ", x);
            }
        }
    }
}
