using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Models;
using VDrumExplorer.Models.Fields;
using VDrumExplorer.Models.TD17;

namespace VDrumExplorer.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int inputId = FindId(InputDevice.DeviceCount, i => InputDevice.GetDeviceCapabilities(i).name, "3- TD-17");
            int outputId = FindId(OutputDevice.DeviceCount, i => OutputDevice.GetDeviceCapabilities(i).name, "3- TD-17");
            using (var client = new SysExClient(inputId, outputId, modelId: 0x4b, deviceId: 17))
            {
                var module = TD17ModuleFields.Load();

                foreach (var kit in module.Kits.Take(2))
                {
                    Console.WriteLine($"Kit {kit.KitNumber}");
                    await PrintFieldSetAsync(client, kit.Common);

                    foreach (var instrument in kit.Instruments.Take(5))
                    {
                        Console.WriteLine($"Instrument {instrument.InstrumentNumber}");
                        await PrintFieldSetAsync(client, instrument.Common);
                    }
                }
            }
        }

        private static async Task PrintFieldSetAsync(SysExClient client, FieldSet fieldSet)
        {
            Console.WriteLine($"{fieldSet.Description}:");
            var data = await client.RequestDataAsync(fieldSet.StartAddress, fieldSet.Size, new CancellationTokenSource(5000).Token);
            foreach (var field in fieldSet.Fields)
            {
                Console.WriteLine($"{field.Name}: {field.ParseSysExData(data)}");
            }
        }

        private static int FindId(int count, Func<int, string> nameFetcher, string name)
        {
            for (int i = 0; i < count; i++)
            {
                if (nameFetcher(i) == name)
                {
                    return i;
                }
            }
            throw new ArgumentException($"Unable to find device {name}");
        }
    }
}
