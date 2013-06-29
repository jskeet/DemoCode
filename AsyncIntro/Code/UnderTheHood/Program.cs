// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnderTheHood
{
    class Program
    {
        static void Main(string[] args)
        {
            Task<int> task = DecompilationSample.SumCharactersAsync("text");
            Console.WriteLine(task.Result);

            //Task task = DummyAsync();
            //task.Wait();
        }

        static async Task DummyAsync()
        {
            RobAwaitable rob = new RobAwaitable();
            string text = await rob;
            Console.WriteLine("I got {0}", text);
            Console.WriteLine(new StackTrace());
        }
    }

    internal class RobAwaitable
    {
        internal RobAwaiter GetAwaiter()
        {
            return new RobAwaiter();
        }
    }

    internal class RobAwaiter : INotifyCompletion
    {
        internal bool IsCompleted { get { return true; } }

        public void OnCompleted(Action continuation)
        {
            Console.WriteLine("This won't get called");
        }

        public string GetResult()
        {
            return "Surprise!";
        }
    }

}
