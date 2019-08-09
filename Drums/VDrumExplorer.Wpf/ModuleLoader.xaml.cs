using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VDrumExplorer.Data;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for ModuleLoader.xaml
    /// </summary>
    public partial class ModuleLoader : Window
    {
        private (SysExClient client, ModuleSchema schema)? detectedMidi;

        public ModuleLoader()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Log("Loading schema registry");
            await Task.Run(SchemaRegistry.GetSchemas);
            var midiDevice = await DetectMidiDeviceAsync();
            detectedMidi = midiDevice;
            loadFromDeviceButton.IsEnabled = detectedMidi.HasValue;
            Log("-----------------");
        }

        private void OnClosed(object sender, EventArgs e)
        {            
            detectedMidi?.client.Dispose();
        }

        private async Task<(SysExClient client, ModuleSchema schema)?> DetectMidiDeviceAsync()
        {
            var inputDevices = DeviceInfo.GetInputDevices();
            var outputDevices = DeviceInfo.GetOutputDevices();

            Log($"Detecting MIDI ports");
            Log($"Input ports:");
            foreach (var input in inputDevices)
            {
                Log($"{input.LocalDeviceId}: {input.Name}");
            }
            Log($"Output ports:");
            foreach (var output in outputDevices)
            {
                Log($"{output.LocalDeviceId}: {output.Name}");
            }
            var commonNames = inputDevices.Select(input => input.Name).Intersect(outputDevices.Select(output => output.Name)).OrderBy(x => x).ToList();
            if (commonNames.Count == 0)
            {
                Log($"Not detected any input/output MIDI ports. Abandoning MIDI detection.");
                return null;
            }
            if (commonNames.Count > 1)
            {
                Log($"Detected multiple input/output MIDI ports: {string.Join(",", commonNames)}. Abandoning MIDI detection.");
                return null;
            }
            string name = commonNames.Single();
            var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
            var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
            if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
            {
                Log($"Matched name {name} is ambiguous. Abandoning MIDI detection.");
                return null;
            }
            Log($"Using MIDI ports with name {name}. Detecting devices using Roland identity requests.");

            var inputId = matchedInputs[0].LocalDeviceId;
            var outputId = matchedOutputs[0].LocalDeviceId;

            ConcurrentBag<IdentityResponse> responses = new ConcurrentBag<IdentityResponse>();
            Log("Before identity");
            using (var identityClient = new IdentityClient(inputId, outputId))
            {
                Log("In identity");
                identityClient.IdentityReceived += response => responses.Add(response);
                identityClient.SendRequests();
                // Half a second should be plenty of time.
                Log("Before delay");
                await Task.Delay(500);
                Log("After delay");
            }
            Log("After identity");
            var schemas = SchemaRegistry.GetSchemas();
            var responseList = responses.OrderBy(r => r.DeviceId).ToList();
            ModuleSchema matchedSchema = null;
            IdentityResponse matchedResponse = null;
            int matchCount = 0;
            foreach (var response in responseList)
            {
                var match = schemas.FirstOrDefault(s => response.FamilyCode == s.FamilyCode && response.FamilyNumberCode == s.FamilyNumberCode);
                string matchLog = match == null ? "No matching schema" : $"Matches schema {match.Name}";
                Log($"Detected device ID {response.DeviceId} with family code {response.FamilyCode} ({response.FamilyNumberCode}) : {matchLog}");
                if (match != null)
                {
                    matchedSchema = match;
                    matchedResponse = response;
                    matchCount++;
                }
            }
            switch (matchCount)
            {
                case 0:
                    Log($"No devices with a known schema. Abandoning MIDI detection.");
                    return null;
                case 1:
                    Log($"Using device {matchedResponse.DeviceId} with schema {matchedSchema.Name}.");
                    return (new SysExClient(inputId, outputId, matchedSchema.ModelId, matchedResponse.DeviceId), matchedSchema);
                default:
                    Log($"Multiple devices with a known schema. Abandoning MIDI detection.");
                    return null;
            }
        }

        private void Log(string text) => logPanel.Text += $"{DateTime.Now:HH:mm:ss.fff} {text}\r\n";

        private void Log(Exception e)
        {
            // TODO: Aggregate exception etc.
            Log(e.ToString());
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {

        }

        private void LoadFromDevice(object sender, RoutedEventArgs e)
        {

        }
    }
}
