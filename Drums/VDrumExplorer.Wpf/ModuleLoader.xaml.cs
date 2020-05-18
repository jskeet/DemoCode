// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Audio;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for ModuleLoader.xaml
    /// </summary>
    public partial class ModuleLoader : Window
    {
        private readonly TextBlockLogger logger;
        private (RolandMidiClient client, ModuleSchema schema)? detectedMidi;

        public ModuleLoader()
        {
            InitializeComponent();
            logger = new TextBlockLogger(logPanel);
            LogVersion();
            Loaded += OnLoaded;
            Closed += OnClosed;
            // We can't attach this event handler in XAML, as only instance members of the current class are allowed.
            loadKitFromDeviceKitNumber.PreviewTextInput += TextConversions.CheckDigits;
        }

        private void LogVersion()
        {
            var version = typeof(ModuleLoader).Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (version != null)
            {
                logger.LogInformation($"V-Drum Explorer version {version.InformationalVersion}");
            }            
            else
            {
                logger.LogWarning($"Version attribute not found.");
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadSchemaRegistry();
            logger.LogInformation("Detecting connected V-Drums modules");
            detectedMidi = null;
            var midiDevice = await MidiDevices.DetectSingleRolandMidiClientAsync(logger, SchemaRegistry.KnownSchemas.Keys);
            if (midiDevice != null)
            {
                detectedMidi = (midiDevice, SchemaRegistry.KnownSchemas[midiDevice.Identifier].Value);
            }            
            midiPanel.IsEnabled = midiDevice is object;
            logger.LogInformation($"Device detection result: {(midiDevice is object ? $"{midiDevice.Identifier.Name} detected" : "no compatible modules (or multiple modules) detected")}");
            logger.LogInformation("-----------------");
        }

        private async Task LoadSchemaRegistry()
        {
            logger.LogInformation($"Loading known schemas");
            foreach (var pair in SchemaRegistry.KnownSchemas)
            {
                logger.LogInformation($"Loading schema for {pair.Key.Name}");
                await Task.Run(() => pair.Value.Value);
            }
            logger.LogInformation($"Finished loading schemas");
        }

        private void OnClosed(object sender, EventArgs e)
        {            
            detectedMidi?.client.Dispose();
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Filter = "All explorer files|*.vdrum;*.vkit;*.vaudio|Module files|*.vdrum|Kit files|*.vkit|Module audio files|*.vaudio" };
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
                logger.LogError($"Error loading {dialog.FileName}", ex);
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
                case ModuleAudio audio:
                {
                    new InstrumentAudioExplorer(logger, audio, dialog.FileName).Show();
                    break;
                }
                default:
                    logger.LogError($"Unknown file data type");
                    break;
            }

            void Validate(Func<Data.Fields.ValidationResult> validationAction)
            {
                logger.LogInformation($"Validating fields");
                var validationResult = validationAction();
                foreach (var error in validationResult.Errors)
                {
                    logger.LogError($"Field {error.Path} error: {error.Message}");
                }
                logger.LogInformation($"Validation complete. Total fields: {validationResult.TotalFields}. Errors: {validationResult.Errors.Count}");
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

        private void RecordInstrumentsFromDevice(object sender, RoutedEventArgs e)
        {
            var schema = detectedMidi.Value.schema;
            var client = detectedMidi.Value.client;
            new SoundRecorderDialog(logger, schema, client).ShowDialog();
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
