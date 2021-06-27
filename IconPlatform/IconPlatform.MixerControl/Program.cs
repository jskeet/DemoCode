using IconPlatform.Model;
using OscMixerControl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IconPlatform.MixerControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // MIDI input name hard-coded as I think the chances of anyone else running this are slim...
            await using (var controller = await PlatformMXController.ConnectAsync("Platform M+ V2.15"))
            {
                Console.WriteLine("Connected to controller");

                using (var mixer = new Mixer())
                {
                    // IP address hard-coded as I think the chances of anyone else running this are slim...
                    mixer.Connect("192.168.1.41", 10024);
                    mixer.RegisterHandler("/info", (sender, message) => Console.WriteLine($"Mixer info response: {string.Join("/", message)}"));
                    await mixer.SendInfoAsync();

                    using (var connector = new MixerConnector(controller, mixer))
                    {
                        await connector.StartAsync();
                        await Task.Delay(Timeout.Infinite);
                    }
                }
            }
        }
    }
}
