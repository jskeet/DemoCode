// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WrappingAsync
{
    public static class TupleExtensions
    {
        public static Task<Tuple<T1, T2, T3>> Transpose<T1, T2, T3>(
            this Tuple<Task<T1>, Task<T2>, Task<T3>> input)
        {
            var tcs = new TaskCompletionSource<Tuple<T1, T2, T3>>();
            Task.WhenAll(input.Item1, input.Item2, input.Item3)
                .ContinueWith(task =>
                {
                    switch (task.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            var result = Tuple.Create(
                                input.Item1.Result,
                                input.Item2.Result,
                                input.Item3.Result);
                            tcs.SetResult(result);
                            break;
                        case TaskStatus.Faulted:
                            tcs.SetException(task.Exception.InnerExceptions);
                            break;
                        case TaskStatus.Canceled:
                            tcs.SetCanceled();
                            break;
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        public static TaskAwaiter<Tuple<T1, T2, T3>> GetAwaiter<T1, T2, T3>(
            this Tuple<Task<T1>, Task<T2>, Task<T3>> input)
        {
            return input.Transpose().GetAwaiter();
        }
    }
}
