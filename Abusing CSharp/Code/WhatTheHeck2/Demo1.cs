// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Linq;

namespace WhatTheHeck2
{
    class Demo1
    {
        static void Main()
        {
            dynamic x = Mystery.GetValue();
            Console.WriteLine(x.Count());
        }
    }
}
