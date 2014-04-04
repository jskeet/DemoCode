// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    /// <summary>
    /// Simple class to control execution: it just keeps executing actions until it's completed.
    /// Awaitables can be created which will always make the generated code yield and pass the state
    /// machine to a handler.
    /// </summary>
    public class Executioner : ITransient
    {
        private readonly Action<Executioner> entryPoint;
        private Action nextAction;

        public Executioner(Action<Executioner> entryPoint)
        {
            this.entryPoint = entryPoint;
        }

        public void Start()
        {
            Execute(entryPoint);
        }

        protected void Execute(Action<Executioner> action)
        {
            nextAction = () => action(this);
            while (nextAction != null)
            {
                Action next = nextAction;
                nextAction = null;
                next();
            }
        }

        public IAwaitable CreateAwaitable(Action<IAsyncStateMachine> stateMachineHandler)
        {
            var awaiter = new YieldingAwaiter(continuation =>
            {
                var machine = AsyncUtil.GetStateMachine(continuation);
                stateMachineHandler(machine);
                nextAction = continuation;
            });
            return awaiter.NewAwaitable();
        }

        public IAwaitable CreateAwaitable(Func<IAsyncStateMachine, Action> stateMachineHandler)
        {
            var awaiter = new YieldingAwaiter(continuation =>
            {
                var machine = AsyncUtil.GetStateMachine(continuation);
                nextAction = stateMachineHandler(machine);
            });
            return awaiter.NewAwaitable();
        }

        public YieldingAwaitable<T> CreateYieldingAwaitable<T>(Action<IAsyncStateMachine> stateMachineHandler, T result)
        {
            var awaiter = new YieldingAwaiter<T>(continuation =>
            {
                var machine = AsyncUtil.GetStateMachine(continuation);
                stateMachineHandler(machine);
                nextAction = continuation;
            }, result);
            return new YieldingAwaitable<T>(awaiter);
        }

        public YieldingAwaitable<T> CreateYieldingAwaitable<T>(Func<IAsyncStateMachine, Action> stateMachineHandler, T value)
        {
            var awaiter = new YieldingAwaiter<T>(continuation =>
            {
                var machine = AsyncUtil.GetStateMachine(continuation);
                nextAction = stateMachineHandler(machine);
            }, value);
            return new YieldingAwaitable<T>(awaiter);
        }
    }
}
