// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

namespace FunWithAwaiters
{
    public static class AwaiterExtensions
    {
        private class Awaitable : IAwaitable
        {
            private readonly IAwaiter awaiter;

            internal Awaitable(IAwaiter awaiter)
            {
                this.awaiter = awaiter;
            }

            public IAwaiter GetAwaiter()
            {
                return awaiter;
            }
        }

        public static IAwaitable NewAwaitable(this IAwaiter awaiter)
        {
            return new Awaitable(awaiter);
        }

        private class Awaitable<T> : IAwaitable<T>
        {
            private readonly IAwaiter<T> awaiter;

            internal Awaitable(IAwaiter<T> awaiter)
            {
                this.awaiter = awaiter;
            }

            public IAwaiter<T> GetAwaiter()
            {
                return awaiter;
            }
        }

        public static IAwaitable<T> NewAwaitable<T>(this IAwaiter<T> awaiter)
        {
            return new Awaitable<T>(awaiter);
        }
    }
}
