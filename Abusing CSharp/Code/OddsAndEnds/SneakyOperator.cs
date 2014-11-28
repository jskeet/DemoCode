// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace OddsAndEnds
{
    class SneakyOperator : ArithmeticOperator
    {
        private static volatile ArithmeticOperator instance;

        private SneakyOperator(int x) : this(() => { throw new Exception(); })
        { }

        private SneakyOperator(Func<int> func) : this(func())
        { }

        public static ArithmeticOperator GetInstance()
        {
            try
            {
                new SneakyOperator(0);
            }
            catch
            { }
            ArithmeticOperator local;
            while ((local = instance) == null)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return local;
        }

        ~SneakyOperator()
        {
            instance = this;
        }

        public override int Apply(int lhs, int rhs)
        {
            return (int) Math.Pow(lhs, rhs); 
        }
    }
}
