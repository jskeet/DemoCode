// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace VDrumExplorer.Blazor
{
    public class PromiseHandler : IDisposable
    {
        public DotNetObjectReference<PromiseHandler> Proxy { get; }
        private readonly TaskCompletionSource<int> tcs;

        public PromiseHandler()
        {
            Proxy = DotNetObjectReference.Create(this);
            tcs = new TaskCompletionSource<int>();
        }

        [JSInvokable]
        public void Success() =>
            tcs.TrySetResult(0);

        [JSInvokable]
        public void Failure(string message) =>
            tcs.TrySetException(new Exception(message));

        public Task Task => tcs.Task;

        public void Dispose() => Proxy.Dispose();
    }
}
