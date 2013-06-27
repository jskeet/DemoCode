// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    public static class AsyncStateMachineExtensions
    {
        public static int GetState(this IAsyncStateMachine stateMachine)
        {
            return (int) stateMachine.GetStateField().GetValue(stateMachine);
        }

        public static void SetState(this IAsyncStateMachine stateMachine, int state)
        {
            stateMachine.GetStateField().SetValue(stateMachine, state);
        }

        public static Action GetActionForState(this IAsyncStateMachine stateMachine, int state)
        {
            return () =>
            {
                stateMachine.SetState(state);
                stateMachine.MoveNext();
            };
        }

        private static FieldInfo GetStateField(this IAsyncStateMachine stateMachine)
        {
            return stateMachine.GetType().GetField("<>1__state");
        }

        public static void SaveTo(this IAsyncStateMachine stateMachine, string file)
        {
            using (var stream = File.Create(file))
            {
                SerializationUtil.SerializeFields(stateMachine, stream, ShouldSerialize);
            }
        }

        public static void LoadFrom(this IAsyncStateMachine target, string file)
        {
            using (var stream = File.OpenRead(file))
            {
                SerializationUtil.DeserializeFields(target, stream);
            }
        }

        public static void SaveTo(this IAsyncStateMachine stateMachine, Stream stream)
        {
            SerializationUtil.SerializeFields(stateMachine, stream, ShouldSerialize);
        }

        public static void LoadFrom(this IAsyncStateMachine target, Stream stream)
        {
            SerializationUtil.DeserializeFields(target, stream);
        }

        // This is all down to experimentation...
        private static bool ShouldSerialize(FieldInfo field)
        {
            if (typeof(ITransient).IsAssignableFrom(field.FieldType))
            {
                return false;
            }

            string name = field.Name;
            // Parameters end up just with the same name as the original.
            // Local variables end up with the identifier in the angle brackets, e.g. <foo>
            if (!name.StartsWith("<>"))
            {
                return true;
            }

            // <>1__state will hold the state, which we do want to serialize.
            // <>7__wrap* holds things like extra variables for iterators and using statements.
            // <>t__builder holds the AsyncTaskMethodBuilder (or whatever) - we don't want that
            // <>u__%awaiter* holds awaiters
            // (More TBD?)
            return char.IsDigit(name[2]);
        }
    }
}
