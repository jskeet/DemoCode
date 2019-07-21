using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Models;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                int inputId = FindId(InputDevice.DeviceCount, i => InputDevice.GetDeviceCapabilities(i).name, "3- TD-17");
                int outputId = FindId(OutputDeviceBase.DeviceCount, i => OutputDeviceBase.GetDeviceCapabilities(i).name, "3- TD-17");
                using (var client = new SysExClient(inputId, outputId, modelId: 0x4b, deviceId: 17))
                {
                    var td17 = ModuleFields.FromAssemblyResources(typeof(ModuleFields).Assembly, "VDrumExplorer.Models.TD17", "TD17.json");
                    var visitor = new LoadingVisitor();
                    visitor.Visit(td17.Root);
                    using (var output = new BinaryWriter(File.Create("td17.dat")))
                    {
                        foreach (var container in visitor.Containers)
                        {
                            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Loading {container.Path}");
                            var data = await client.RequestDataAsync(container.Address, container.Size, new CancellationTokenSource(5000).Token);
                            output.Write(container.Address);
                            output.Write(container.Size);
                            output.Write(data);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class LoadingVisitor : FieldVisitor
        {
            public List<Container> Containers { get; } = new List<Container>();

            public override void VisitContainer(Container container)
            {
                if (container.Loadable)
                {
                    Containers.Add(container);
                    //Console.WriteLine($"Would load container {container.Name} at address {container.Address:x}");
                }
            }
        }

        /*
        private static async Task PrintFieldSetAsync(SysExClient client, FieldSet fieldSet)
        {
            Console.WriteLine($"{fieldSet.Description}:");
            var data = await client.RequestDataAsync(fieldSet.StartAddress, fieldSet.Size, new CancellationTokenSource(5000).Token);
            foreach (var field in fieldSet.Fields)
            {
                Console.WriteLine($"{field.Name}: {field.ParseSysExData(data)}");
            }
        }*/

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
