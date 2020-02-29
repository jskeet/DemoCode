// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Audio;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for SoundRecorderDialog.xaml
    /// </summary>
    public partial class SoundRecorderDialog : Window
    {
        private readonly ILogger logger;
        private readonly ModuleSchema schema;
        private readonly RolandMidiClient midiClient;
        private readonly CancellationTokenSource cancellationTokenSource;
        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        private string outputFile = null;

        public SoundRecorderDialog()
        {
            InitializeComponent();
        }

        internal SoundRecorderDialog(ILogger logger, ModuleSchema schema, RolandMidiClient midiClient)
            : this()
        {
            this.logger = logger;
            this.schema = schema;
            this.midiClient = midiClient;
            kitNumber.PreviewTextInput += TextConversions.CheckDigits;
            userSamples.PreviewTextInput += TextConversions.CheckDigits;
            cancellationTokenSource = new CancellationTokenSource();
            inputDevice.ItemsSource = AudioDevices.GetInputDeviceNames();
            kitNumber.Text = TextConversions.Format(schema.KitRoots.Count);
            foreach (var group in schema.InstrumentGroups)
            {
                instrumentGroupSelector.Items.Add(group.Description);
            }
        }

        private async void StartRecording(object sender, RoutedEventArgs args)
        {
            var config = TryCreateConfig();
            // Shouldn't really happen, as the button shouldn't be enabled.
            if (config == null)
            {
                return;
            }

            // Load all the details 
            var instrumentRoot = config.KitRoot.DescendantNodesAndSelf().FirstOrDefault(node => node.InstrumentNumber == 1);
            if (instrumentRoot == null)
            {
                logger.Log($"No instrument root available. Please email a bug report to skeet@pobox.com");
                return;
            }

            var data = new ModuleData();
            var midiNoteChain = instrumentRoot.MidiNoteField;
            if (midiNoteChain == null)
            {
                logger.Log($"No midi field available. Please email a bug report to skeet@pobox.com");
                return;
            }

            logger.Log($"Starting recording process");
            var midiNoteContext = midiNoteChain.GetFinalContext(instrumentRoot.Context);
            logger.Log($"Loading existing data to restore after recording");
            List<FixedContainer> instrumentContainers;
            try
            {
                await LoadContainerAsync(data, midiNoteContext);
                instrumentContainers = instrumentRoot.DescendantNodesAndSelf()
                    .SelectMany(node => node.Details)
                    .Select(detail => detail.Container)
                    .Where(fc => fc != null)
                    .Distinct()
                    .ToList();
                foreach (var container in instrumentContainers)
                {
                    await LoadContainerAsync(data, container);
                }
            }
            catch (Exception e)
            {
                logger.Log($"Error loading data for recording", e);
                return;
            }

            data.Snapshot();

            var (instrumentFieldContext, instrumentField) =
                (from ct in instrumentContainers
                 orderby ct.Address
                 from field in ct.Container.Fields
                 where field is InstrumentField
                 select (ct, (InstrumentField) field)).FirstOrDefault();
            if (instrumentFieldContext == null)
            {
                logger.Log($"No instrument field available. Please email a bug report to skeet@pobox.com");
                return;
            }

            var midiNote = midiNoteChain.FinalField.GetMidiNote(midiNoteContext, data);
            if (midiNote == null)
            {
                logger.Log($"No midi note for instrument 1. Please email a bug report to skeet@pobox.com");
            }

            int velocity = (int) attackSlider.Value;
            var instrumentGroupToRecord = (string) instrumentGroupSelector.SelectedItem;
            var instrumentsToRecord = schema.PresetInstruments
                .Where(ins => instrumentGroupSelector.SelectedIndex == 0 || ins.Group.Description == instrumentGroupToRecord)
                .ToList();
            progress.Maximum = instrumentsToRecord.Count + config.UserSamples;

            logger.Log($"Starting recording process");
            try
            {
                var captures = new List<InstrumentAudio>();
                progress.Value = 0;
                foreach (var instrument in instrumentsToRecord)
                {
                    var instrumentAudio = await RecordInstrument(instrument);
                    captures.Add(instrumentAudio);
                }
                for (int i = 0; i < config.UserSamples; i++)
                {
                    var instrumentAudio = await RecordInstrument(schema.UserSampleInstruments[i]);
                    captures.Add(instrumentAudio);
                }
                var moduleAudio = new ModuleAudio(schema, AudioDevices.AudioFormat, config.RecordingDuration, captures.AsReadOnly());
                using (var output = File.Create(config.OutputFile))
                {
                    moduleAudio.Save(output);
                }
                logger.Log($"Saved instrument sounds to {config.OutputFile}.");
            }
            catch (OperationCanceledException)
            {
                logger.Log("Cancelled recording");
            }
            catch (Exception e)
            {
                logger.Log($"Error recording data", e);
            }
            finally
            {
                data.RevertSnapshot();
                await RestoreData(data);
            }
            Close();

            async Task<InstrumentAudio> RecordInstrument(Instrument instrument)
            {
                progress.Value++;
                progressLabel.Content = $"Recording {instrument.Name}";
                foreach (var container in instrumentContainers)
                {
                    container.Container.Reset(container, data);
                }
                instrumentField.SetInstrument(instrumentFieldContext, data, instrument);
                foreach (var container in instrumentContainers)
                {
                    var segment = data.GetSegment(container.Address);
                    midiClient.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40, CancellationToken);
                }
                await Task.Delay(200, CancellationToken);
                midiClient.Silence(config.MidiChannel);
                await Task.Delay(40);
                var recordingTask = AudioDevices.RecordAudio(config.AudioDeviceId, config.RecordingDuration, CancellationToken);
                midiClient.PlayNote(config.MidiChannel, midiNote.Value, velocity);
                await Task.Delay(2500);
                var audio = await recordingTask;
                return new InstrumentAudio(instrument, audio);
            }
        }

        private async Task RestoreData(ModuleData data)
        {
            logger.Log("Restoring original data");
            try
            {
                foreach (var segment in data.GetSegments())
                {
                    midiClient.SendData(segment.Start.Value, segment.CopyData());
                    // Note: no cancellation token; the token may already have been cancelled!
                    await Task.Delay(40);
                }
            }
            catch (Exception e)
            {
                logger.Log($"Error restoring data", e);
            }
        }

        private async Task LoadContainerAsync(ModuleData data, FixedContainer context)
        {
            if (data.GetSegmentOrNull(context.Address) != null)
            {
                return;
            }
            var timerToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, timerToken).Token;
            var segment = await midiClient.RequestDataAsync(context.Address.Value, context.Container.Size, effectiveToken);
            data.Populate(context.Address, segment);
        }

        /// <summary>
        /// The cancel button always just acts like the Close button. We'll cancel
        /// any current recording in HandleClosing.
        /// </summary>
        private void Cancel(object sender, RoutedEventArgs e) => Close();

        private void HandleClosing(object sender, CancelEventArgs e) =>
            cancellationTokenSource.Cancel();

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "V-Drum Explorer audio files|*.vaudio" };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            outputFile = dialog.FileName;
            fileLabel.Content = outputFile;
            MaybeEnableRecordButton();
        }

        private void MaybeEnableRecordButton() =>
            recordButton.IsEnabled = TryCreateConfig() != null;

        private void KitNumberChanged(object sender, TextChangedEventArgs e) =>
            MaybeEnableRecordButton();

        private void InputDeviceChanged(object sender, SelectionChangedEventArgs e) =>
            MaybeEnableRecordButton();

        private Config TryCreateConfig()
        {
            if (inputDevice.SelectedIndex == -1)
            {
                return null;
            }
            string deviceName = (string) inputDevice.Items[inputDevice.SelectedIndex];
            int? deviceId = AudioDevices.GetAudioInputDeviceId(deviceName);
            if (deviceId == null)
            {
                return null;
            }
            int midiChannel = int.Parse(midiChannelSelector.Text);
            if (!TextConversions.TryParseDecimal(recordingTime.Text, out var recordingSeconds))
            {
                return null;
            }
            TimeSpan recordingDuration = TimeSpan.FromSeconds((double) recordingSeconds);
            if (!TextConversions.TryGetKitRoot(kitNumber.Text, schema, logger, out var kit))
            {
                return null;
            }
            if (outputFile == null)
            {
                return null;
            }
            if (!TextConversions.TryParseInt32(userSamples.Text, out int parsedUserSamples))             
            {
                return null;
            }
            if (parsedUserSamples < 0 || parsedUserSamples > schema.UserSampleInstruments.Count)
            {
                return null;
            }

            return new Config
            {
                KitRoot = kit,
                RecordingDuration = recordingDuration,
                OutputFile = outputFile,
                AudioDeviceId = deviceId.Value,
                UserSamples = parsedUserSamples,
                MidiChannel = midiChannel,
                Attack = (int) attackSlider.Value
            };
        }

        private sealed class Config
        {
            internal VisualTreeNode KitRoot { get; set; }
            internal TimeSpan RecordingDuration { get; set; }
            internal string OutputFile { get; set; }
            internal int AudioDeviceId { get; set; }
            internal int UserSamples { get; set; }
            internal int MidiChannel { get; set; }
            internal int Attack { get; set; }
        }
    }
}
