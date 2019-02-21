using System;
using System.Threading.Tasks;

namespace Async
{
    class AsyncDisposal
    {
        class AsyncResource : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                Console.WriteLine("Disposing asynchronously...");
                await Task.Delay(2000);
                Console.WriteLine("... done");
            }

            public async Task PerformWorkAsync()
            {
                Console.WriteLine("Performing work asynchronously...");
                await Task.Delay(2000);
                Console.WriteLine("... done");
            }
        }

        async static Task Main()
        {
            await using (var resource = new AsyncResource())
            {
                await resource.PerformWorkAsync();
            }
            Console.WriteLine("After the await using statement");
        }
    }
}
