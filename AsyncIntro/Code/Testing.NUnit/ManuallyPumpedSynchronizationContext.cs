// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Testing.NUnit
{
    /// <summary>
    /// A synchronization context which will store delegates to execute, and allow them
    /// to be "pumped" explicitly.
    /// </summary>
    public sealed class ManuallyPumpedSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<Tuple<SendOrPostCallback, object>> callbacks;

        public ManuallyPumpedSynchronizationContext()
        {
            callbacks = new BlockingCollection<Tuple<SendOrPostCallback, object>>();
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            callbacks.Add(Tuple.Create(callback, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("Synchronous operations not supported on ManuallyPumpedSynchronizationContext");
        }

        public void PumpAll()
        {
            Tuple<SendOrPostCallback, object> callback;
            while(callbacks.TryTake(out callback))
            {
                callback.Item1(callback.Item2);
            }
        }
    }
}
