using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VDrumExplorer.Midi;
using VDrumExplorer.Models;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Gui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var data = LoadDataFromFile(@"c:\users\jon\test\projects\democode\drums\td17.dat");
            Application.Run(new ModuleExplorer(data));
        }


        private static ModuleData LoadDataFromFile(string path)
        {
            var td17 = LoadTd17ModuleFields();
            var data = new ModuleData(td17);
            using (var stream = File.OpenRead(path))
            {
                data.Load(stream);
            }
            return data;
        }

        private static ModuleFields LoadTd17ModuleFields() =>
            ModuleFields.FromAssemblyResources(typeof(ModuleFields).Assembly, "VDrumExplorer.Models.TD17", "TD17.json");

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
