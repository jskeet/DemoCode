// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace UnderTheHood
{
    // These don't really exist...
    // Interfaces for asynchronous operations returning values
    public interface IAwaitable<T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<T>
    {
        bool IsCompleted { get; }
        void OnCompleted(Action continuation);
        T GetResult();
    }



    // Interfaces for "void" asynchronous operations
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter
    {
        bool IsCompleted { get; }
        void OnCompleted(Action continuation);
        void GetResult();
    }
}
