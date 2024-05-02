using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using XTouchMini.Model;

namespace XTouchMini.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using (var controller = await XTouchMiniStandardController.ConnectAsync(NullLogger.Instance, "X-TOUCH MINI"))
            {
                System.Console.WriteLine("Connected");
                controller.ButtonDown += (sender, args) => System.Console.WriteLine($"Button {args.Button} in {args.Layer} pressed");
                controller.ButtonUp += (sender, args) => System.Console.WriteLine($"Button {args.Button} in {args.Layer} released");

                controller.KnobDown += (sender, args) => System.Console.WriteLine($"Knob {args.Knob} in {args.Layer} pressed");
                controller.KnobUp += (sender, args) => System.Console.WriteLine($"Knob {args.Knob} in {args.Layer} released");

                controller.KnobTurned += (sender, args) => System.Console.WriteLine($"Knob {args.Knob} in {args.Layer} turned to position {args.Value}");
                controller.FaderMoved += (sender, args) => System.Console.WriteLine($"Fader {args.Layer} moved to position {args.Position}");

                controller.ButtonDown += (sender, args) =>
                {
                    int button = args.Button;
                    switch (button)
                    {
                            // Buttons 1-5 (top row) set the knob ring style
                            case >= 1 and <= 6:
                            var style = (KnobRingStyle) (button - 1);
                            System.Console.WriteLine($"Setting style to {style}");
                            for (int i = 1; i <= 8; i++)
                            {
                                controller.SetKnobRingStyle(i, style);
                            }
                            break;
                            // Buttons 9 and 10 (leftmost of bottom row) change the layer.
                            // This leaves the button lights in an odd state, but never mind.
                            case >= 9 and <= 10:
                            var layer = (Layer) (button - 8);
                            System.Console.WriteLine($"Setting active layer to {layer}");
                            controller.SetActiveLayer(layer);
                            break;
                            // Buttons 11-16 (remainder of bottom row) test various knob ring values:
                            // 11: All on
                            // 12: All blinking
                            // 13: Light 3 on
                            // 14: Light 3 blinking
                            // 15: All off (via LedState)
                            // 16: All off (via value)
                            case 11:
                            controller.SetKnobRingLights(1, LedState.On, 14);
                            break;
                        case 12:
                            controller.SetKnobRingLights(1, LedState.Blinking, 14);
                            break;
                        case 13:
                            controller.SetKnobRingLights(1, LedState.On, 3);
                            break;
                        case 14:
                            controller.SetKnobRingLights(1, LedState.Blinking, 3);
                            break;
                        case 15:
                            controller.SetKnobRingLights(1, LedState.Off, 3);
                            break;
                        case 16:
                            controller.SetKnobRingLights(1, LedState.On, 0);
                            break;
                    };

                };

                while (true)
                {
                    System.Console.WriteLine("Enter MIDI bytes (space separated):");
                    var line = System.Console.ReadLine();
                    var bytes = line.Split(' ').Select(part => Convert.ToByte(part, 16)).ToArray();
                    controller.SendMidiMessage(bytes);
                }
            }
        }
    }
}
