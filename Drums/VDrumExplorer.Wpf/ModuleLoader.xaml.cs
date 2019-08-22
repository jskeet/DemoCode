// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
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
        private readonly TextBlockLogger logger;
        private (SysExClient client, ModuleSchema schema)? detectedMidi;

        public ModuleLoader()
        {
            InitializeComponent();
            logger = new TextBlockLogger(logPanel);
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            logger.Log("Loading schema registry");
            await Task.Run(SchemaRegistry.GetSchemas);
            var midiDevice = await DetectMidiDeviceAsync();
            detectedMidi = midiDevice;
            loadFromDeviceButton.IsEnabled = detectedMidi.HasValue;
            logger.Log("-----------------");
        }

        private void OnClosed(object sender, EventArgs e)
        {            
            detectedMidi?.client.Dispose();
        }

        private async Task<(SysExClient client, ModuleSchema schema)?> DetectMidiDeviceAsync()
        {
            var inputDevices = DeviceInfo.GetInputDevices();
            var outputDevices = DeviceInfo.GetOutputDevices();

            logger.Log($"Detecting MIDI ports");
            logger.Log($"Input ports:");
            foreach (var input in inputDevices)
            {
                logger.Log($"{input.LocalDeviceId}: {input.Name}");
            }
            logger.Log($"Output ports:");
            foreach (var output in outputDevices)
            {
                logger.Log($"{output.LocalDeviceId}: {output.Name}");
            }
            var commonNames = inputDevices.Select(input => input.Name).Intersect(outputDevices.Select(output => output.Name)).OrderBy(x => x).ToList();
            if (commonNames.Count == 0)
            {
                logger.Log($"Not detected any input/output MIDI ports. Abandoning MIDI detection.");
                return null;
            }
            if (commonNames.Count > 1)
            {
                logger.Log($"Detected multiple input/output MIDI ports: {string.Join(",", commonNames)}. Abandoning MIDI detection.");
                return null;
            }
            string name = commonNames.Single();
            var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
            var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
            if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
            {
                logger.Log($"Matched name {name} is ambiguous. Abandoning MIDI detection.");
                return null;
            }
            logger.Log($"Using MIDI ports with name {name}. Detecting devices using Roland identity requests.");

            var inputId = matchedInputs[0].LocalDeviceId;
            var outputId = matchedOutputs[0].LocalDeviceId;

            ConcurrentBag<IdentityResponse> responses = new ConcurrentBag<IdentityResponse>();
            using (var identityClient = new IdentityClient(inputId, outputId))
            {
                identityClient.IdentityReceived += response => responses.Add(response);
                identityClient.SendRequests();
                // Half a second should be plenty of time.
                await Task.Delay(500);
            }
            var schemas = SchemaRegistry.GetSchemas();
            var responseList = responses.OrderBy(r => r.DeviceId).ToList();
            ModuleSchema matchedSchema = null;
            IdentityResponse matchedResponse = null;
            int matchCount = 0;
            foreach (var response in responseList)
            {
                var match = schemas.FirstOrDefault(s => response.FamilyCode == s.FamilyCode && response.FamilyNumberCode == s.FamilyNumberCode);
                string matchLog = match == null ? "No matching schema" : $"Matches schema {match.Name}";
                logger.Log($"Detected device ID {response.DeviceId} with family code {response.FamilyCode} ({response.FamilyNumberCode}) : {matchLog}");
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
                    logger.Log($"No devices with a known schema. Abandoning MIDI detection.");
                    return null;
                case 1:
                    logger.Log($"Using device {matchedResponse.DeviceId} with schema {matchedSchema.Name}.");
                    return (new SysExClient(inputId, outputId, matchedSchema.ModelId, matchedResponse.DeviceId), matchedSchema);
                default:
                    logger.Log($"Multiple devices with a known schema. Abandoning MIDI detection.");
                    return null;
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            string fileName;
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            fileName = dialog.FileName;

            Module module;
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    module = Module.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading {fileName}", ex);
                return;
            }
            logger.Log($"Validating fields");
            var validation = module.Validate();
            foreach (var error in validation.Errors)
            {
                logger.Log($"Field {error.Field.Path} error: {error.Message}");
            }
            logger.Log($"Validation complete. Total fields: {validation.TotalFields}. Errors: {validation.Errors.Count}");
            var client = detectedMidi?.schema == module.Schema ? detectedMidi?.client : null;
            new ModuleExplorer(logger, module, client).Show();
        }

        private void LoadFromDevice(object sender, RoutedEventArgs e)
        {
            // Shouldn't happen, as the button shouldn't be enabled.
            if (detectedMidi == null)
            {
                return;
            }
            var midi = detectedMidi.Value;
            var dialog = new DeviceLoaderDialog(logger, midi.client, midi.schema);
            var result = dialog.ShowDialog();
            if (result == true)
            {
                new ModuleExplorer(logger, dialog.Module, midi.client).Show();
            }
        }

        private void SaveLog(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Text files|*.txt" };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            logger.SaveLog(dialog.FileName);

        }
    }
}
