// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnderTheHood
{
    class DecompilationSample
    {
        public static async Task<int> SumCharactersAsync(IEnumerable<char> text)
        {
            int total = 0;
            foreach (char ch in text)
            {
                int unicode = ch;
                await Task.Delay(unicode);
                total += unicode;
            }

            await Task.Yield();

            return total;
        }
    }
}
