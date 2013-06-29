// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.NUnit
{
    public sealed class TimeMachine
    {
        private readonly SortedDictionary<int, Action> actions = new SortedDictionary<int, Action>();

        public Task<T> ScheduleSuccess<T>(int time, T value)
        {
            return AddAction<T>(time, tcs => tcs.SetResult(value));
        }

        public Task<T> ScheduleFault<T>(int time, Exception exception)
        {
            return AddAction<T>(time, tcs => tcs.SetException(exception));
        }

        public Task<T> ScheduleFault<T>(int time, IEnumerable<Exception> exceptions)
        {
            return AddAction<T>(time, tcs => tcs.SetException(exceptions));
        }

        public Task<T> ScheduleCancellation<T>(int time)
        {
            return AddAction<T>(time, tcs => tcs.SetCanceled());
        }

        private Task<T> AddAction<T>(int time, Action<TaskCompletionSource<T>> action)
        {
            if (time <= 0)
            {
                throw new ArgumentOutOfRangeException("time", "Tasks can only be scheduled with a positive time");
            }
            if (actions.ContainsKey(time))
            {
                throw new ArgumentException("A task completing at this time has already been scheduled.", "time");
            }
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            actions[time] = () => action(source);
            return source.Task;
        }

        public void ExecuteInContext(Action<Advancer> action)
        {
            ExecuteInContext(new ManuallyPumpedSynchronizationContext(), action);
        }

        public void ExecuteInContext(ManuallyPumpedSynchronizationContext context, Action<Advancer> action)
        {
            SynchronizationContext originalContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                Advancer advancer = new Advancer(actions, context);
                // This is where the tests assertions etc will go...
                action(advancer);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        // So tempted to call this class SonicScrewdriver...
        public class Advancer
        {
            private readonly SortedDictionary<int, Action> actions;
            private readonly ManuallyPumpedSynchronizationContext context;
            private int time = 0;

            internal Advancer(SortedDictionary<int, Action> actions, ManuallyPumpedSynchronizationContext context)
            {
                this.actions = actions;
                this.context = context;
            }

            public int Time { get { return time; } }

            /// <summary>
            /// Advances to the given target time.
            /// </summary>
            /// <param name="targetTime"></param>
            public void AdvanceTo(int targetTime)
            {
                if (targetTime <= time)
                {
                    throw new ArgumentOutOfRangeException("targetTime", "Can only advance time forwards");
                }
                List<int> timesToRemove = new List<int>();
                foreach (var entry in actions.TakeWhile(e => e.Key <= targetTime))
                {
                    timesToRemove.Add(entry.Key);
                    entry.Value();
                    context.PumpAll();
                }
                foreach (int key in timesToRemove)
                {
                    actions.Remove(key);
                }
                time = targetTime;
            }

            /// <summary>
            /// Advances the clock by the given number of arbitrary time units
            /// </summary>
            /// <param name="amount"></param>
            public void AdvanceBy(int amount)
            {
                if (amount <= 0)
                {
                    throw new ArgumentOutOfRangeException("amount", "Can only advance time forwards");
                }
                AdvanceTo(time + amount);
            }

            /// <summary>
            /// Advances the clock by one time unit.
            /// </summary>
            public void Advance()
            {
                AdvanceBy(1);
            }
        }
    }
}
