using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Example2b
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            var client = await StorageClient.CreateAsync();

            var buckets = client.ListBucketsAsync("jonskeet-uberproject");
            using (var iterator = buckets.GetEnumerator())
            {
                while (await iterator.MoveNext())
                {
                    var bucket = iterator.Current;
                    Console.WriteLine(bucket.Name);
                }
            }
        }
    }
}
