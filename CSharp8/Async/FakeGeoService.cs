using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Async
{
    class FakeGeoService : IGeoService
    {
        private LinkedList<Page> pages = new LinkedList<Page>();

        public void AddPage(params string[] cities)
        {
            // The first page always has a null token; that's what the client sends first.
            string pageToken = pages.Count == 0 ? null : Guid.NewGuid().ToString();
            pages.AddLast(new Page(pageToken, cities.ToList()));
        }

        public async Task<ListCitiesResponse> ListCitiesAsync(ListCitiesRequest request)
        {
            // Simulate looking up in a database etc
            await Task.Delay(2000);
            LinkedListNode<Page> node = pages.First;
            while (node != null)
            {
                var page = node.Value;
                if (page.PageToken == request.PageToken)
                {
                    return new ListCitiesResponse(node.Next?.Value.PageToken, page.Cities);
                }
                node = node.Next;
            }
            throw new ArgumentException("Unknown page token");
        }

        private class Page
        {
            internal string PageToken { get; }
            internal List<string> Cities { get; }

            internal Page(string token, List<string> cities) =>
                (PageToken, Cities) = (token, cities);
        }
    }
}
