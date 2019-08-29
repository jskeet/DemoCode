using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Async
{
    class AsyncIterator
    {
        static async Task Main()
        {
            await foreach (var item in ListStringsAsync())
            {
                Console.WriteLine(item);
            }
        }

        private static async IAsyncEnumerable<string> ListStringsAsync()
        {
            yield return "First";
            await Task.Delay(1000);
            yield return "Second";
            await Task.Delay(1000);
            yield return "Third";
        }
    }
}
