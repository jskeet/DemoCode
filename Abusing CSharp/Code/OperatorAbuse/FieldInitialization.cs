// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace OperatorAbuse
{
    class FieldInitialization
    {
        private GenericType<Dictionary<string, int>, IDynamicMetaObjectProvider, ICriticalNotifyCompletion> foo;
        private Dictionary<IDynamicMetaObjectProvider, ICriticalNotifyCompletion> bar;

        public FieldInitialization()
        {
            // Would rather not have this...
            foo = new GenericType<Dictionary<string, int>, IDynamicMetaObjectProvider, ICriticalNotifyCompletion>();

            // Can have this instead!
            foo = +foo;

            // Would rather not have this...
            bar = new Dictionary<IDynamicMetaObjectProvider, ICriticalNotifyCompletion>();

            // Can have this instead...
            bar = bar.New();
        }
    }

    public static class Extensions
    {
        public static Dictionary<TKey, TValue> New<TKey, TValue>(this Dictionary<TKey, TValue> ignored)
        {
            return new Dictionary<TKey, TValue>();
        }
    }

    class GenericType<T1, T2, T3>
    {
        public static GenericType<T1, T2, T3> operator +(GenericType<T1, T2, T3> ignored)
        {
            return new GenericType<T1, T2, T3>();
        }
    }

}
