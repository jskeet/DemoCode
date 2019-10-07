﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
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
            LogVersion();
            Loaded += OnLoaded;
            Loaded += LoadSchemaRegistry;
            Closed += OnClosed;
            // We can't attach this event handler in XAML, as only instance members of the current class are allowed.
            loadKitFromDeviceKitNumber.PreviewTextInput += TextConversions.CheckDigits;
        }

        private void LogVersion()
        {
            var version = typeof(ModuleLoader).Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (version != null)
            {
                logger.Log($"V-Drum Explorer version {version.InformationalVersion}");
            }            
            else
            {
                logger.Log($"Version attribute not found.");
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var midiDevice = await DetectMidiDeviceAsync();
            detectedMidi = midiDevice;
            midiPanel.IsEnabled = detectedMidi.HasValue;
            logger.Log("-----------------");
        }

        private async void LoadSchemaRegistry(object sender, RoutedEventArgs e)
        {
            logger.Log($"Loading known schemas");
            foreach (var pair in SchemaRegistry.KnownSchemas)
            {
                logger.Log($"Loading schema for {pair.Key.Name}");
                await Task.Run(() => pair.Value.Value);
            }
            logger.Log($"Finished loading schemas");
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
            var schemaKeys = SchemaRegistry.KnownSchemas.Keys;
            var responseList = responses.OrderBy(r => r.DeviceId).ToList();
            ModuleIdentifier matchedIdentifier = null;
            IdentityResponse matchedResponse = null;
            int matchCount = 0;
            foreach (var response in responseList)
            {
                var match = schemaKeys.FirstOrDefault(s => response.FamilyCode == s.FamilyCode && response.FamilyNumberCode == s.FamilyNumberCode);
                string matchLog = match == null ? "No matching schema" : $"Matches schema {match.Name}";
                logger.Log($"Detected device ID {response.DeviceId} with family code {response.FamilyCode} ({response.FamilyNumberCode}) : {matchLog}");
                if (match != null)
                {
                    matchedIdentifier = match;
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
                    logger.Log($"Using device {matchedResponse.DeviceId} with schema {matchedIdentifier.Name}.");
                    var schema = SchemaRegistry.KnownSchemas[matchedIdentifier].Value;
                    return (new SysExClient(inputId, outputId, matchedIdentifier.ModelId, matchedResponse.DeviceId), schema);
                default:
                    logger.Log($"Multiple devices with a known schema. Abandoning MIDI detection.");
                    return null;
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Filter = "All explorer files|*.vdrum;*.vkit|Module files|*.vdrum|Kit files|*.vkit" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            object loaded;
            try
            {
                using (var stream = File.OpenRead(dialog.FileName))
                {
                    loaded = SchemaRegistry.ReadStream(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading {dialog.FileName}", ex);
                return;
            }
            // TODO: Potentially declare an IDrumData interface with the Schema property and Validate method.
            switch (loaded)
            {
                case Kit kit:
                {
                    Validate(kit.Validate);
                    var client = detectedMidi?.schema == kit.Schema ? detectedMidi?.client : null;
                    new KitExplorer(logger, kit, client, dialog.FileName).Show();
                    break;
                }
                case Data.Module module:
                {
                    Validate(module.Validate);
                    var client = detectedMidi?.schema == module.Schema ? detectedMidi?.client : null;
                    new ModuleExplorer(logger, module, client, dialog.FileName).Show();
                    break;
                }
                default:
                    logger.Log($"Unknown file data type");
                    break;
            }

            void Validate(Func<Data.Fields.ValidationResult> validationAction)
            {
                logger.Log($"Validating fields");
                var validationResult = validationAction();
                foreach (var error in validationResult.Errors)
                {
                    logger.Log($"Field {error.Path} error: {error.Message}");
                }
                logger.Log($"Validation complete. Total fields: {validationResult.TotalFields}. Errors: {validationResult.Errors.Count}");
            }
        }

        private void LoadModuleFromDevice(object sender, RoutedEventArgs e)
        {
            // Shouldn't happen, as the button shouldn't be enabled.
            if (detectedMidi == null)
            {
                return;
            }
            var midi = detectedMidi.Value;
            var schema = midi.schema;
            try
            {
                var dialog = new DeviceLoaderDialog(logger, midi.client, schema);
                dialog.LoadDeviceData(schema.Root);
                midiPanel.IsEnabled = false;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    new ModuleExplorer(logger, new Data.Module(schema, dialog.Data), midi.client, fileName: null).Show();
                }
            }
            finally
            {
                midiPanel.IsEnabled = true;
            }
        }

        private void LoadKitFromDevice(object sender, RoutedEventArgs e)
        {
            // Shouldn't happen, as the button shouldn't be enabled.
            if (detectedMidi == null)
            {
                return;
            }
            var midi = detectedMidi.Value;
            var schema = midi.schema;

            if (!TextConversions.TryGetKitRoot(loadKitFromDeviceKitNumber.Text, schema, logger, out var specifiedKitRoot))
            {
                return;
            }

            midiPanel.IsEnabled = false;
            try
            {
                var dialog = new DeviceLoaderDialog(logger, midi.client, schema);
                dialog.LoadDeviceData(specifiedKitRoot.Context);
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    var firstKitRoot = schema.KitRoots[1];
                    var clonedData = specifiedKitRoot.Context.CloneData(dialog.Data, firstKitRoot.Context.Address);
                    var kit = new Kit(schema, clonedData, specifiedKitRoot.KitNumber.Value);
                    new KitExplorer(logger, kit, midi.client, fileName: null).Show();
                }
            }
            finally
            {
                midiPanel.IsEnabled = true;
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
