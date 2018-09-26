using System;
using System.Threading.Tasks;

namespace Demos
{
    public class GeoServiceDemo
    {
        async static Task Main()
        {
            var fakeService = new FakeGeoService();
            fakeService.AddPage("London", "Paris", "Washington");
            fakeService.AddPage(); // Empty pages are valid
            fakeService.AddPage("Nairobi", "Sydney", "New Delhi");
            fakeService.AddPage("Moscow");

            var client = new GeoClient(fakeService);

            foreach await (var city in client.ListCitiesAsync())
            {
                Console.WriteLine(city);
            }
        }
    }
}
