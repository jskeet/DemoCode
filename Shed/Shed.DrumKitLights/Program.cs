using Sanford.Multimedia.Midi;
using System;

namespace Shed.DrumKitLights
{
    class Program
    {
        private static int[] noteMapping = new int[128];

        static void Main(string[] args)
        {
            for (int i = 0; i < 128; i++)
            {
                noteMapping[i] = i;
            }

            // Specific mappings
            noteMapping[36] = 0x10; // Kick
            noteMapping[38] = 0x20; // Snare head
            noteMapping[40] = 0x30; // Snare rim
            noteMapping[37] = 0x40; // Snare xstick
            noteMapping[48] = 0x50; // Tom 1 head
            noteMapping[50] = 0x60; // Tom 1 rim
            noteMapping[45] = 0x70; // Tom 2 head
            noteMapping[47] = 0x80; // Tom 2 rim
            noteMapping[43] = 0x90; // Tom 3 head
            noteMapping[58] = 0xa0; // Tom 3 rim
            noteMapping[46] = 0xb0; // Hi-hat open bow
            noteMapping[26] = 0xc0; // Hi-hat open edge
            noteMapping[42] = 0xd0; // Hi-hat closed bow
            noteMapping[22] = 0xe0; // Hi-hat closed edge
            noteMapping[49] = 0x05; // Crash 1 bow
            noteMapping[55] = 0x45; // Crash 1 edge
            noteMapping[51] = 0x85; // Ride bow
            noteMapping[59] = 0xc5; // Ride edge

            // No settings for aux, crash 2, ride bell

            var controller = new LightingController();
            controller.On();
            controller.White();

            using (var device = new InputDevice(0))
            {
                device.ChannelMessageReceived += (sender, e) => ReactToMessage(controller, e.Message);
                device.StartRecording();
                Console.WriteLine("Hit return to quit.");
                Console.ReadLine();
            }
        }

        private static void ReactToMessage(LightingController controller, ChannelMessage message)
        {
            if (message.Command == ChannelCommand.NoteOn)
            {
                int note = message.Data1;
                int velocity = message.Data2;
                controller.Hue(noteMapping[note]);
                controller.Brightness(velocity / 5);
                Console.WriteLine($"Note: {note}; Velocity: {velocity}");
            }
        }
    }
}
