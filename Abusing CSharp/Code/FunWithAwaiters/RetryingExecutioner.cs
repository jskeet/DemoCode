// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FunWithAwaiters
{
    public class RetryingExecutioner : ITransient
    {
        private Action machineContinuation;
        private IAsyncStateMachine stateMachine;

        private byte[] savedState;
        private int retryCount;
        private int retryMax;
        private readonly Func<RetryingExecutioner, Task> entryPoint;

        public RetryingExecutioner(Func<RetryingExecutioner, Task> entryPoint)
        {
            this.entryPoint = entryPoint;
        }

        public void Start()
        {
            Task task = entryPoint(this);
            while (true)
            {
                if (task.Status == TaskStatus.RanToCompletion || task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    retryCount++;
                    if (savedState == null || machineContinuation == null || stateMachine == null || retryCount == retryMax)
                    {
                        Console.WriteLine("                Retry limit exceeded. Rethrowing...");
                        throw task.Exception.InnerExceptions[0];
                    }
                    //Console.WriteLine("Handled exception: {0}", task.Exception.InnerExceptions[0].Message);
                    Console.WriteLine("                Retrying: {0} out of {1}", retryCount, retryMax);
                    task = entryPoint(this);
                }
                else
                {
                    machineContinuation();                    
                }
            }
        }

        public IAwaitable Setup()
        {
            var awaiter = new YieldingAwaiter(continuation =>
            {
                stateMachine = AsyncUtil.GetStateMachine(continuation);
                machineContinuation = continuation;
                if (savedState == null)
                {
                    SaveState(stateMachine);
                }
                else
                {
                    stateMachine.LoadFrom(new MemoryStream(savedState));
                }
            });
            return awaiter.NewAwaitable();
        }

        public IAwaitable Retry(int times)
        {
            var awaiter = new YieldingAwaiter(continuation =>
            {
                retryCount = 0;
                retryMax = times;
                SaveState(stateMachine);
            });
            return awaiter.NewAwaitable();
        }

        private void SaveState(IAsyncStateMachine stateMachine)
        {
            using (var stream = new MemoryStream())
            {
                stateMachine.SaveTo(stream);
                savedState = stream.ToArray();
            }
        }
    }
}
