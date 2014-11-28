// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class SneakyOperatorDemo
    {
        static void Main()
        {
            ArithmeticOperator op = SneakyOperator.GetInstance();
            Console.WriteLine(op.Apply(2, 5));
        }
    }
}
