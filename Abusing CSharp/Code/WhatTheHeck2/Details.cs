// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections;
using System.Collections.Generic;

namespace WhatTheHeck2
{
    public class Mystery
    {
        public static System.Linq.dynamic GetValue()
        {
            return new Static();
        }
    }

    public class Static : System.Linq.dynamic
    {
        public int Count()
        {
            return 10;
        }
    }
}

namespace System.Linq
{
    public class dynamic : IEnumerable<int>
    {        
        public IEnumerator<int> GetEnumerator()
        {
 	        yield return 1;
            yield return 2;
            yield return 3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
 	        throw new NotImplementedException();
        }
    }
}
