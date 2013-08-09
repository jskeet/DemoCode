// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;

namespace OddsAndEnds
{
    class WhereAsEnumerable
    {
        static void Main()
        {
            // Pretend this is really a db context or whatever
            IQueryable<string> source = new string[0].AsQueryable();

            var query = from x in source
                        where x.StartsWith("Foo") // Queryable.Where
                        where QueryHacks.TransferToClient
                        where x.GetHashCode() == 5 // Enumerable.Where
                        select x.Length;
        }
    }

    public static class QueryHacks
    {
        public static readonly HackToken TransferToClient = HackToken.Instance;

        public static IEnumerable<T> Where<T>(
            this IQueryable<T> source,
            Func<T, HackToken> ignored)
        {
            // Just like AsEnumerable... we're just changing the compile-time
            // type, effectively.
            return source;
        }

        // This class only really exists to make sure we don't *accidentally* use
        // the hack above.
        public class HackToken
        {
            internal static readonly HackToken Instance = new HackToken();
            private HackToken() { }
        }
    }

}
