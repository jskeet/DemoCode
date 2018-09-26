using System;
using System.ComponentModel;

namespace Demos
{
    class AsyncIterator
    {
        private readonly IGeoService service;

        public AsyncIterator(IGeoService service) =>
            this.service = service;

        /*
         * Expected code: does not yet compile.
        public async IAsyncEnumerable<string> ListCitiesAsync()
        {
            string pageToken = null;
            do
            {
                var request = new ListCitiesRequest(pageToken);
                var response = await service.ListCitiesAsync(request);
                foreach (var city in response.Cities)
                {
                    yield return city;
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);
        }
        */
    }
}
