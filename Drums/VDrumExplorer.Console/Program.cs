using Sanford.Multimedia.Midi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Just so we can write synchronous code when we want to
            await Task.Yield();
            try
            {
                await CopyFromKitToFile(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class DumpingVisitor : FieldVisitor
        {
            private readonly ModuleData data;

            public DumpingVisitor(ModuleData data) => this.data = data;

            public override void VisitIPrimitiveField(IPrimitiveField field)
            {
                if (data.HasData(field.Address))
                {
                    Console.WriteLine($"{field.Address}: {field.Path}: {field.GetText(data)}");
                }
                else
                {
                    Console.WriteLine($"No data for {field.Address}: {field.Path} - bad config?");
                }
            }

            public override void VisitContainer(Container container)
            {
                if (data.HasData(container.Address) || !container.Loadable)
                {
                    base.VisitContainer(container);
                }
                else
                {
                    Console.WriteLine($"No data for {container.Path}");
                }
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
                base.VisitContainer(container);
            }
        }

        private static ModuleData LoadDataFromFile(string path)
        {
            var td17 = LoadTd17ModuleSchema();
            var data = new ModuleData(td17);
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                // Note: this may not be reliable due to buffering. Use it for now...
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int address = reader.ReadInt32();
                    int size = reader.ReadInt32();
                    // FIXME: Dump without the wrong size.
                    if (size >= 0x100)
                    {
                        size -= 0x80;
                    }
                    byte[] bytes = reader.ReadBytes(size);
                    data.Populate(new ModuleAddress(address), bytes);
                }
            }
            return data;
        }

        public static async Task CopyFromKitToFile(string path)
        {
            using (var client = CreateClientForTd17())
            {
                var td17 = LoadTd17ModuleSchema();
                var visitor = new LoadingVisitor();
                visitor.Visit(td17.Root);
                var moduleData = new ModuleData(td17);
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Loading {visitor.Containers.Count} containers");
                await LoadContainersParallel(client, visitor.Containers, moduleData);
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Loaded");
                using (var output = File.Create(path))
                {
                    moduleData.Save(output);
                }
            }
        }

        private static async Task LoadContainersSerial(SysExClient client, List<Container> containers, ModuleData moduleData)
        {
            foreach (var container in containers)
            {
                var data = await client.RequestDataAsync(container.Address.Value, container.Size, new CancellationTokenSource(5000).Token);
                moduleData.Populate(container.Address, data);
            }
        }

        private static Task LoadContainersParallel(SysExClient client, List<Container> containers, ModuleData moduleData)
        {
            return ForEachAsync(containers, 5, async container =>
            {
                var data = await client.RequestDataAsync(container.Address.Value, container.Size, new CancellationTokenSource(5000).Token);
                moduleData.Populate(container.Address, data);
            });
        }

        private static Task ForEachAsync<T>(IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));
        }

        private static ModuleSchema LoadTd17ModuleSchema() =>
            ModuleSchema.FromAssemblyResources(typeof(ModuleSchema).Assembly, "VDrumExplorer.Data.TD17", "TD17.json");

        private static SysExClient CreateClientForTd17()
        {
            int inputId = FindId(InputDevice.DeviceCount, i => InputDevice.GetDeviceCapabilities(i).name, "3- TD-17");
            int outputId = FindId(OutputDeviceBase.DeviceCount, i => OutputDeviceBase.GetDeviceCapabilities(i).name, "3- TD-17");
            return new SysExClient(inputId, outputId, modelId: 0x4b, deviceId: 17);
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
