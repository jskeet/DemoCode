using System.Collections.Generic;

namespace Async
{
    public class GeoClient
    {
        private readonly IGeoService service;

        public GeoClient(IGeoService service) =>
            this.service = service;

        public async IAsyncEnumerable<string> ListCitiesAsync()
        {
            string nextPageToken = null;
            do
            {
                var response = await service.ListCitiesAsync(new ListCitiesRequest(nextPageToken));
                foreach (var city in response.Cities)
                {
                    yield return city;
                }
                nextPageToken = response.NextPageToken;
            } while (nextPageToken != null);
        }
    }
}
