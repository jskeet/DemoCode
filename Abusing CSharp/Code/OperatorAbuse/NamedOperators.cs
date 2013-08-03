// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
// Based on ideas by Konrad Rudolph: https://github.com/klmr/named-operator
using System;
using System.Collections.Generic;
using System.Linq;

namespace OperatorAbuse
{
    class NamedOperators
    {
        static void Main()
        {
            var repeat = Operators.Repeat<string>();
            var join = Operators.Join<string>();
            var result = "Hello" <repeat> 3 <join> ",";
            Console.WriteLine(result);
        }
    }

    public static class Operators
    {
        public static Operator<T, int, IEnumerable<T>> Repeat<T>()
        {
            return new Operator<T, int, IEnumerable<T>>(Enumerable.Repeat);
        }

        public static Operator<IEnumerable<T>, string, string> Join<T>()
        {
            return new Operator<IEnumerable<T>, string, string>((values, separator) => string.Join(separator, values));
        }
    }

    public class Operator<TLeft, TRight, TResult>
    {
        private readonly Func<TLeft, TRight, TResult> func;

        public Operator(Func<TLeft, TRight, TResult> func)
        {
            this.func = func;
        }
        
        public static PartialOperator<TLeft, TRight, TResult> operator <(TLeft lhs, Operator<TLeft, TRight, TResult> op)
        {
            return new PartialOperator<TLeft, TRight, TResult>(lhs, op.func);
        }

        public static PartialOperator<TLeft, TRight, TResult> operator >(TLeft lhs, Operator<TLeft, TRight, TResult> op)
        {
            return new PartialOperator<TLeft, TRight, TResult>(lhs, op.func);
        }
    }

    public class PartialOperator<TLeft, TRight, TResult>
    {
        private readonly Func<TLeft, TRight, TResult> func;
        private readonly TLeft left;

        internal PartialOperator(TLeft left, Func<TLeft, TRight, TResult> func)
        {
            this.left = left;
            this.func = func;
        }

        public static TResult operator >(PartialOperator<TLeft, TRight, TResult> op, TRight rhs)
        {
            return op.func(op.left, rhs);
        }

        public static TResult operator <(PartialOperator<TLeft, TRight, TResult> op, TRight rhs)
        {
            return op.func(op.left, rhs);
        }
    }
}
