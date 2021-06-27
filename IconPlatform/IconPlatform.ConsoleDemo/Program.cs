using IconPlatform.Model;
using System;
using System.Threading.Tasks;

namespace IconPlatform.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var controller = await PlatformMXController.ConnectAsync("Platform M+ V2.15");

            controller.ButtonChanged += (sender, args) =>
            {
                Console.WriteLine($"Channel {args.Channel} button {args.Button} {(args.Down ? "pressed" : "released")}");
                controller.SetLight(args.Channel, args.Button, args.Down);
            };
            controller.KnobTurned += (sender, args) =>
            {
                Console.WriteLine($"Channel {args.Channel} knob turned with value {args.Value}");
            };
            controller.FaderMoved += (sender, args) =>
            {
                Console.WriteLine($"Channel {args.Channel} fader moved to {args.Position}");
                controller.MoveFader(args.Channel, args.Position);
            };
            await Task.Delay(60000);
        }
    }
}
