// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WrappingAsync
{
    public static class AsyncExtensions
    {
        public static Task<dynamic> TransposeAnonymous<T>(this T source) where T : class
        {
            // TODO: Lots of validation :)
            Type type = source.GetType();
            Type genericDefinition = type.GetGenericTypeDefinition();
            // Convert Anon<Task<int>, Task<string>> to Anon<int, string>
            Type constructedType = genericDefinition.MakeGenericType(type.GetGenericArguments().Select(x => x.GetGenericArguments()[0]).ToArray());

            // Fetch the property names from the constructor parameter here, so that the tasks are in the right
            // order to invoke the constructor later, when we fetch the results.
            var tasks = type.GetConstructors()[0].GetParameters()
                            .Select(p => type.GetProperty(p.Name).GetValue(source))
                            .Cast<Task>()
                            .ToArray();

            Task whenAll = Task.WhenAll(tasks);
            var tcs = new TaskCompletionSource<dynamic>();
            whenAll.ContinueWith(task =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        var ctor = constructedType.GetConstructors()[0];
                        var args = tasks.Select(t => ((dynamic)t).Result)
                                        .ToArray();
                        tcs.SetResult(ctor.Invoke(args));
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

        public static TaskAwaiter<dynamic> GetAwaiter<T>(this T source) where T : class
        {
            return TransposeAnonymous(source).GetAwaiter();
        }
    }
}
