// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
namespace OddsAndEnds
{
    public abstract class ArithmeticOperator
    {
        private static readonly ArithmeticOperator add = new AddOperator();
        private static readonly ArithmeticOperator subtract = new SubtractOperator();
        private static readonly ArithmeticOperator multiply = new MultiplyOperator();
        private static readonly ArithmeticOperator divide = new DivideOperator();

        public static ArithmeticOperator Add { get { return add; } }
        public static ArithmeticOperator Subtract { get { return subtract; } }
        public static ArithmeticOperator Multiply { get { return multiply; } }
        public static ArithmeticOperator Divide { get { return divide; } }

        // Prevent non-nested subclasses. They can't call this constructor,
        // which is the only way of creating an instance.
        private ArithmeticOperator()
        { }

        public abstract int Apply(int lhs, int rhs);

        private class AddOperator : ArithmeticOperator
        {
            public override int Apply(int lhs, int rhs)
            {
                return lhs + rhs;
            }
        }

        private class SubtractOperator : ArithmeticOperator
        {
            public override int Apply(int lhs, int rhs)
            {
                return lhs - rhs;
            }
        }

        private class MultiplyOperator : ArithmeticOperator
        {
            public override int Apply(int lhs, int rhs)
            {
                return lhs * rhs;
            }
        }

        private class DivideOperator : ArithmeticOperator
        {
            public override int Apply(int lhs, int rhs)
            {
                return lhs / rhs;
            }
        }
    }
}
