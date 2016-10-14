using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NotInPreview3
{
    class AsyncReturn
    {
        static async CustomTask<int> FooAsync()
        {
            Console.WriteLine("Before await");
            await Task.Delay(5000);
            Console.WriteLine("After await");
            return 10;
        }
    }

    public class CustomTask<T>
    {
        public static CustomTaskBuilder CreateAsyncMethodBuilder() =>
            new CustomTaskBuilder();

        public CustomTaskAwaiter GetAwaiter() =>
            new CustomTaskAwaiter();

        public class CustomTaskBuilder
        {
            public void SetStateMachine(IAsyncStateMachine stateMachine) { }

            public void Start<TSM>(ref TSM stateMachine) where TSM : IAsyncStateMachine
            { }

            public void AwaitOnCompleted<TA, TSM>(ref TA awaiter, ref TSM stateMachine)
                where TA : INotifyCompletion where TSM : IAsyncStateMachine
            { }

            public void AwaitUnsafeOnCompleted<TA, TSM>(ref TA awaiter, ref TSM stateMachine)
                where TA : ICriticalNotifyCompletion where TSM : IAsyncStateMachine
            { }

            public void SetResult(T result) { }
            public void SetException(Exception ex) { }
            public CustomTask<T> Task { get; }
        }

        public class CustomTaskAwaiter : INotifyCompletion
        {
            public void OnCompleted(Action continuation)
            {                
            }

            public bool IsCompleted { get; }

            public T GetResult() => default(T);            
        }
    }
}
