// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WrappingAsync
{
    public static class TupleExtensions
    {
        public static async Task<Tuple<T1, T2, T3>> Transpose<T1, T2, T3>(
            this Tuple<Task<T1>, Task<T2>, Task<T3>> input)
        {
            return Tuple.Create(await input.Item1, await input.Item2, await input.Item3);
        }

        public static TaskAwaiter<Tuple<T1, T2, T3>> GetAwaiter<T1, T2, T3>(
            this Tuple<Task<T1>, Task<T2>, Task<T3>> input)
        {
            return input.Transpose().GetAwaiter();
        }
    }
}
