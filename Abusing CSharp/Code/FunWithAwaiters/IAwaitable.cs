// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

namespace FunWithAwaiters
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<T>
    {
        IAwaiter<T> GetAwaiter();
    }
}
