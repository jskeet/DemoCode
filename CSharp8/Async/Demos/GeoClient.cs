using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demos
{
    public class GeoClient
    {
        private readonly IGeoService service;

        public GeoClient(IGeoService service) =>
            this.service = service;

        public IAsyncEnumerable<string> ListCitiesAsync() =>
            new CityEnumerable(service);

        private class CityEnumerable : IAsyncEnumerable<string>
        {
            private readonly IGeoService service;

            public CityEnumerable(IGeoService service) =>
                this.service = service;

            public IAsyncEnumerator<string> GetAsyncEnumerator() =>
                new CityEnumerator(service);
        }

        private class CityEnumerator : IAsyncEnumerator<string>
        {
            private readonly IGeoService service;
            private ListCitiesResponse currentResponse;
            private int nextIndex = 0;

            public CityEnumerator(IGeoService service) =>
                this.service = service;

            public async Task<bool> WaitForNextAsync()
            {
                if (currentResponse != null && currentResponse.NextPageToken == null)
                {
                    return false;
                }
                string nextPageToken = currentResponse?.NextPageToken;
                currentResponse = await service.ListCitiesAsync(new ListCitiesRequest(nextPageToken));
                nextIndex = 0;
                return true;
            }

            public string TryGetNext(out bool success)
            {
                if (nextIndex < currentResponse.Cities.Count)
                {
                    string city = currentResponse.Cities[nextIndex];
                    nextIndex++;
                    success = true;
                    return city;
                }
                else
                {
                    success = false;
                    return null;
                }
            }
        }
    }
}
