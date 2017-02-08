// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    class AsyncMethodBuilderAttribute : Attribute
    {
        public AsyncMethodBuilderAttribute(Type t) { }
    }
}

namespace CSharp7
{
    class AsyncReturn
    {
        static async CustomTask<int> CustomAsync()
        {
            Console.WriteLine("Before await");
            await Task.Delay(5000);
            Console.WriteLine("After await");
            return 10;
        }

        static async Task RegularAsync()
        {
            int result = await CustomAsync();
            Console.WriteLine(result);
        }

        static void Main()
        {
            RegularAsync().Wait();
        }
    }

    [AsyncMethodBuilder(typeof(CustomTaskMethodBuilder<>))]
    struct CustomTask<T>
    {
        internal T _result;
        public T Result => _result;
        internal Awaiter GetAwaiter() => new Awaiter(this);
        internal class Awaiter : INotifyCompletion
        {
            private readonly CustomTask<T> _task;
            internal Awaiter(CustomTask<T> task) { _task = task; }
            public void OnCompleted(Action a) { }
            internal bool IsCompleted => true;
            internal T GetResult() => _task.Result;
        }
    }

    struct CustomTaskMethodBuilder<T>
    {
        private CustomTask<T> _task;
        public static CustomTaskMethodBuilder<T> Create() =>
            new CustomTaskMethodBuilder<T>(new CustomTask<T>());
        internal CustomTaskMethodBuilder(CustomTask<T> task)
        {
            _task = task;
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            Console.WriteLine("SetStateMachineCalled");
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("Start called");
            stateMachine.MoveNext();
        }

        public void SetException(Exception e)
        {
            Console.WriteLine("SetException called");
        }

        public void SetResult(T t)
        {
            Console.WriteLine("SetResult called");
            _task._result = t;
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("AwaitOnCompleted called");
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("AwaitUnsafeOnCompleted called");
        }

        public CustomTask<T> Task => _task;
    }
}
