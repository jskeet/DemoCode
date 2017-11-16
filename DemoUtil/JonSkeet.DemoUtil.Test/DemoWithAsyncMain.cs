using System;
using System.Threading.Tasks;

namespace JonSkeet.DemoUtil.Test
{
    class DemoWithAsyncMain
    {
        static async Task Main()
        {
            Console.WriteLine("Before delay");
            await Task.Delay(1000);
            Console.WriteLine("After delay");
        }
    }
}
