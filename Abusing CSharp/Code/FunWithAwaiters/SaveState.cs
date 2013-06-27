// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    class SaveState
    {
        static void Main(string[] args)
        {
            var executioner = new Executioner(SaveResumeDemo);
            executioner.Start();
        }

        static async void SaveResumeDemo(Executioner executioner)
        {
            if (File.Exists("state.dat"))
            {
                await executioner.CreateAwaitable(machine => machine.LoadFrom("state.dat"));
                Console.WriteLine("We'll never get here");
            }
            Console.WriteLine("Looks like this is the first time through");
            string startTime = DateTime.Now.ToString();
            for (int i = 0; i < 10; i++)
            {
                await executioner.CreateAwaitable(machine => machine.SaveTo("state.dat"));
                Console.WriteLine("Started at: {0}", startTime);
                Console.WriteLine("i = {0}. Continue? ", i);
                string text = Console.ReadLine();
                if (text == "n")
                {
                    return;
                }
            }
            Console.WriteLine("Completed!");
            File.Delete("state.dat");
        }
    }

    internal class SimpleAwaiter : IAwaiter<IAsyncStateMachine>
    {
        private IAsyncStateMachine machine;

        public bool IsCompleted
        {
            get { return false; }
        }

        public IAsyncStateMachine GetResult()
        {
            return machine;
        }

        public void OnCompleted(Action continuation)
        {
            machine = AsyncUtil.GetStateMachine(continuation);
            continuation();
        }
    }
}
