// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnderTheHood
{
    class LogicalStackDecompiled
    {
        [DebuggerStepThrough]
        [AsyncStateMachine(typeof(DemoStateMachine))]
        public static Task DemonstrateStacks()
        {
            var machine = new DemoStateMachine();
            machine.builder = AsyncTaskMethodBuilder.Create();
            machine.state = -1;
            machine.builder.Start(ref machine);
            return machine.builder.Task;
        }

        [CompilerGenerated]
        private struct DemoStateMachine : IAsyncStateMachine
        {            
            // Fields for local variables
            public int x;
            public int y;
            public Task<int> z;
            public Task<int> task;

            // Fields for awaiters
            private TaskAwaiter<int> awaiter;

            // Common infrastructure
            public int state;
            public AsyncTaskMethodBuilder builder;
            private object stack;

            void IAsyncStateMachine.MoveNext()
            {
                try
                {
                    bool doFinallyBodies = true;
                    TaskAwaiter<int> localAwaiter;
                    int localLhs;

                    switch (state)
                    {
                        case -3:
                            goto Done;
                        case 0:
                            goto FirstAwaitContinuation;
                        case 1:
                            goto SecondAwaitContinuation;
                    }
                    // Default case - first call (state = -1)
                    y = 10;
                    z = Task.FromResult(10);
                    localAwaiter = z.GetAwaiter();
                    localLhs = y;
                    if (localAwaiter.IsCompleted)
                    {
                        goto FirstAwaitCompletion;
                    }
                    stack = localLhs;
                    state = 0;
                    awaiter = localAwaiter;
                    builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    doFinallyBodies = false;
                    return;
                FirstAwaitContinuation:
                    localLhs = (int)stack;
                    stack = null;
                    localAwaiter = awaiter;
                    awaiter = default(TaskAwaiter<int>);
                    state = -1;
                FirstAwaitCompletion:
                    int localRhs = localAwaiter.GetResult();
                    x = localLhs * localRhs;

                    // Second section of code...
                    task = Task.FromResult(20);

                    string localArg0 = "{0} {1}";
                    int localArg1 = x;
                    localAwaiter = task.GetAwaiter();
                    if (localAwaiter.IsCompleted)
                    {
                        goto SecondAwaitCompletion;
                    }
                    var localTuple = new Tuple<string, int>(localArg0, localArg1);
                    stack = localTuple;
                    state = 1;
                    awaiter = localAwaiter;
                    builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    doFinallyBodies = false;
                    return;
                SecondAwaitContinuation:
                    localTuple = (Tuple<string, int>) stack;
                    localArg0 = localTuple.Item1;
                    localArg1 = localTuple.Item2;
                    localAwaiter = awaiter;
                    awaiter = default(TaskAwaiter<int>);
                    state = -1;
                SecondAwaitCompletion:
                    int localArg2 = localAwaiter.GetResult();
                    Console.WriteLine(localArg0, localArg1, localArg2);
                }
                catch (Exception ex)
                {
                    state = -2;
                    builder.SetException(ex);
                    return;
                }
            Done:
                state = -2;
                builder.SetResult();
            }

            [DebuggerHidden]
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine machine)
            {
                builder.SetStateMachine(machine);
            }
        }
    }
}
