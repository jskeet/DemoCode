// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;

namespace FunWithAwaiters
{
    public sealed class YieldingAwaiter : IAwaiter
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
}
