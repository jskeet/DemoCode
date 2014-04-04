// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;

namespace FunWithAwaiters
{
    public struct YieldingAwaiter : IAwaiter
    {
        private readonly Action<Action> onCompletedHandler;

        public YieldingAwaiter(Action<Action> onCompletedHandler)
        {
            this.onCompletedHandler = onCompletedHandler;
        }

        public bool IsCompleted { get { return false; } }

        public void GetResult()
        {            
        }

        public void OnCompleted(Action continuation)
        {
            onCompletedHandler(continuation);
        }
    }

    public class YieldingAwaitable<T>
    {
        private readonly YieldingAwaiter<T> awaiter;

        internal YieldingAwaitable(YieldingAwaiter<T> awaiter)
        {
            this.awaiter = awaiter;
        }

        public YieldingAwaiter<T> GetAwaiter()
        {
            return awaiter;
        }
    }

    public struct YieldingAwaiter<T> : IAwaiter<T>
    {
        private readonly Action<Action> onCompletedHandler;
        private readonly T result;

        public YieldingAwaiter(Action<Action> onCompletedHandler, T result)
        {
            this.onCompletedHandler = onCompletedHandler;
            this.result = result;
        }

        public bool IsCompleted { get { return false; } }

        public T GetResult()
        {
            return result;
        }

        public void OnCompleted(Action continuation)
        {
            onCompletedHandler(continuation);
        }
    }
}
