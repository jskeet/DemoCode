// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnderTheHood
{
    class DecompilationSampleDecompiled
    {
        [DebuggerStepThrough]
        [AsyncStateMachine(typeof(DemoStateMachine))]
        public static Task<int> SumCharactersAsync(IEnumerable<char> text)
        {
            var machine = new DemoStateMachine();
            machine.text = text;
            machine.builder = AsyncTaskMethodBuilder<int>.Create();
            machine.state = -1;
            machine.builder.Start(ref machine);
            return machine.builder.Task;
        }

        [CompilerGenerated]
        private struct DemoStateMachine : IAsyncStateMachine
        {
            public int state;
            public IEnumerator<char> iterator;
            public AsyncTaskMethodBuilder<int> builder;
            public object stack;
            public TaskAwaiter taskAwaiter;
            public YieldAwaitable.YieldAwaiter yieldAwaiter;
            public char ch;
            public int total;
            public int unicode;
            public IEnumerable<char> text;

            void IAsyncStateMachine.MoveNext()
            {
                int result = default(int);
                try
                {
                    bool doFinallyBodies = true;
                    switch (state)
                    {
                        case -3:
                            goto Done;
                        case 0:
                            goto FirstAwaitContinuation;
                        case 1:
                            goto SecondAwaitContinuation;
                    }
                    // Default case - first call (state is -1)
                    total = 0;
                    iterator = text.GetEnumerator();

                // We really want to jump straight to FirstAwaitRealContinuation, but we can't
                // goto a label inside a try block...
                FirstAwaitContinuation:
                    // foreach loop
                    try
                    {
                        // for/foreach loops typically have the condition at the end of the generated code.
                        // We want to go there *unless* we're trying to reach the first continuation.
                        if (state != 0)
                        {
                            goto LoopCondition;
                        }
                        goto FirstAwaitRealContinuation;
                    LoopBody:
                        ch = iterator.Current;
                        unicode = ch;
                        TaskAwaiter localTaskAwaiter = Task.Delay(unicode).GetAwaiter();
                        if (localTaskAwaiter.IsCompleted)
                        {
                            goto FirstAwaitCompletion;
                        }
                        state = 0;
                        taskAwaiter = localTaskAwaiter;
                        builder.AwaitUnsafeOnCompleted(ref localTaskAwaiter, ref this);
                        doFinallyBodies = false;
                        return;
                    FirstAwaitRealContinuation:
                        localTaskAwaiter = taskAwaiter;
                        taskAwaiter = default(TaskAwaiter);
                        state = -1;
                    FirstAwaitCompletion:
                        localTaskAwaiter.GetResult();
                        localTaskAwaiter = default(TaskAwaiter);
                        total += unicode;
                    LoopCondition:
                        if (iterator.MoveNext())
                        {
                            goto LoopBody;
                        }
                    }
                    finally
                    {
                        if (doFinallyBodies && iterator != null)
                        {
                            iterator.Dispose();
                        }
                    }

                    // After the loop
                    YieldAwaitable.YieldAwaiter localYieldAwaiter = Task.Yield().GetAwaiter();
                    if (localYieldAwaiter.IsCompleted)
                    {
                        goto SecondAwaitCompletion;
                    }
                    state = 1;
                    yieldAwaiter = localYieldAwaiter;
                    builder.AwaitUnsafeOnCompleted(ref localYieldAwaiter, ref this);
                    doFinallyBodies = false;
                    return;

                SecondAwaitContinuation:
                    localYieldAwaiter = yieldAwaiter;
                    yieldAwaiter = default(YieldAwaitable.YieldAwaiter);
                    state = -1;
                SecondAwaitCompletion:
                    localYieldAwaiter.GetResult();
                    localYieldAwaiter = default(YieldAwaitable.YieldAwaiter);
                    result = total;
                }
                catch (Exception ex)
                {
                    state = -2;
                    builder.SetException(ex);
                    return;
                }
            Done:
                state = -2;
                builder.SetResult(result);
            }

            [DebuggerHidden]
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine machine)
            {
                builder.SetStateMachine(machine);
            }
        }
    }
}
