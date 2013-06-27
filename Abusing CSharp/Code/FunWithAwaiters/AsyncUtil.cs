// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    public static class AsyncUtil
    {
        public static IAsyncStateMachine GetStateMachine(Action continuation)
        {
            var target = continuation.Target;
            var field = target.GetType().GetField("m_stateMachine", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IAsyncStateMachine) field.GetValue(target);
        }
    }
}
