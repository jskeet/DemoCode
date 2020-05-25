// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Device;
using VDrumExplorer.ViewModel.Audio;
using static VDrumExplorer.Proto.ModelExtensions;

namespace VDrumExplorer.ViewModel.Dialogs
{
    public class InstrumentAudioRecorderViewModel : ViewModelBase
    {
        private CancellationTokenSource? cancellationTokenSource;
        private readonly ModuleSchema schema;
        private readonly DeviceController device;
        private ILogger logger;
        public InstrumentAudioRecorderSettingsViewModel Settings { get; }
        public InstrumentAudioRecorderProgressViewModel Progress { get; }
        public CommandBase StartRecordingCommand { get; }
        public CommandBase CancelCommand { get; }
        public string Title { get; }

        public InstrumentAudioRecorderViewModel(IViewServices viewServices, ILogger logger, DeviceViewModel deviceViewModel)
        {
            this.logger = logger;
            device = deviceViewModel.ConnectedDevice ?? throw new InvalidOperationException("Cannot record audio without a connected device");
            schema = device.Schema;

            Settings = new InstrumentAudioRecorderSettingsViewModel(viewServices, schema, device.InputName);
            Progress = new InstrumentAudioRecorderProgressViewModel();
            Title = $"Instrument Audio Recorder ({schema.Identifier.Name})";
            StartRecordingCommand = new DelegateCommand(StartRecording, false);
            CancelCommand = new DelegateCommand(Cancel, false);
            Settings.PropertyChanged += (sender, args) => UpdateButtonStatus();
        }

        public bool SettingsEnabled => !CancelCommand.Enabled;
        public bool ProgressEnabled => CancelCommand.Enabled;

        private void UpdateButtonStatus()
        {
            CancelCommand.Enabled = cancellationTokenSource is object;
            StartRecordingCommand.Enabled = cancellationTokenSource is null && Settings.OutputFile is object && Settings.SelectedInputDevice is object;
            RaisePropertyChanged(nameof(SettingsEnabled));
            RaisePropertyChanged(nameof(ProgressEnabled));
        }

        /// <summary>
        /// Cancel the recording operation, if it's still active. This is always safe to call.
        /// </summary>
        public void Cancel() => cancellationTokenSource?.Cancel();

        private async void StartRecording()
        {
            cancellationTokenSource = new CancellationTokenSource();
            UpdateButtonStatus();
            try
            {
                await StartRecording(cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Progress.CurrentInstrumentRecording = e is OperationCanceledException ? "Recording cancelled" : "Error - see log";
            }
            finally
            {
                cancellationTokenSource = null;
                UpdateButtonStatus();
            }
        }

        public async Task StartRecording(CancellationToken token)
        {
            int midiChannel = Settings.SelectedMidiChannel;
            if (Settings.SelectedInputDevice is null)
            {
                return;
            }
            int? audioDeviceId = AudioDevices.GetAudioInputDeviceId(Settings.SelectedInputDevice);
            if (audioDeviceId is null)
            {
                return;
            }
            

            var originalKit = await device.GetCurrentKitAsync(token);
            logger.LogInformation($"Changing from kit {originalKit} to {Settings.KitNumber}");
            await device.SetCurrentKitAsync(Settings.KitNumber, token);
            ModuleAudio moduleAudio;
            try
            {
                moduleAudio = await RecordInstruments(midiChannel, audioDeviceId.Value, token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"Recording operation canceled");
                Progress.CurrentInstrumentRecording = "Recording cancelled";
                return;
            }
            catch (Exception e)
            {
                logger.LogError($"Recording operation failed", e);
                Progress.CurrentInstrumentRecording = "Error - see log";
                return;
            }
            finally
            {
                logger.LogInformation($"Changing kit back to {originalKit}");
                await device.SetCurrentKitAsync(originalKit, token);
            }
            Progress.CurrentInstrumentRecording = "Complete";
            logger.LogInformation($"Recording complete");

            using (var output = File.Create(Settings.OutputFile))
            {
                moduleAudio.Save(output);
            }
            logger.LogInformation($"Saved instrument sounds to {Settings.OutputFile}.");
        }

        private async Task<ModuleAudio> RecordInstruments(int midiChannel, int audioDeviceId, CancellationToken token)
        {
            int attack = Settings.Attack;
            TimeSpan duration = TimeSpan.FromSeconds((double) Settings.RecordingTime);
            int kitNumber = Settings.KitNumber;

            var selectedInstrumentGroup = schema.InstrumentGroups.FirstOrDefault(ig => ig.Description == Settings.SelectedInstrumentGroup);
            var instrumentsToRecord = schema.PresetInstruments.Where(inst => selectedInstrumentGroup is null || inst.Group == selectedInstrumentGroup)
                .Concat(schema.UserSampleInstruments.Take(Settings.UserSamples))
                .ToList();

            Progress.TotalInstruments = instrumentsToRecord.Count;
            Progress.CompletedInstruments = 0;

            Progress.CurrentInstrumentRecording = "Backing up kit data";
            logger.LogInformation($"Loading data from kit {kitNumber} to restore later");
            var savedKit = await device.LoadKitAsync(kitNumber, null, token);

            // Load the details for the whole kit.
            // We don't need all of it, but it doesn't take *that* long, compared with the rest of the process.
            var schemaKitRoot = schema.KitRoots[kitNumber - 1];


            var blankKit = ModuleData.FromLogicalRootNode(schemaKitRoot);
            int midiNote = FindMidiNote(blankKit);
            var captures = new List<InstrumentAudio>();
            try
            {
                Progress.CurrentInstrumentRecording = "Setting kit to all default settings";
                logger.LogInformation($"Setting kit {kitNumber} to all default settings");
                await device.SaveDescendants(blankKit.LogicalRoot, targetAddress: null, progressHandler: null, token);

                logger.LogInformation($"Recording {instrumentsToRecord.Count} instruments");
                foreach (var instrument in instrumentsToRecord)
                {
                    var capture = await RecordInstrument(instrument);
                    captures.Add(capture);
                }
            }
            finally
            {
                Progress.CurrentInstrumentRecording = "Restoring kit data";
                logger.LogInformation($"Restoring snapshot to kit {kitNumber}");                
                /*
                await device.SaveDescendants(
                    savedKit.Data.LogicalRoot,
                    targetAddress: schemaKitRoot.Container.Address,
                    progressHandler: null,
                    CancellationToken.None); // Don't cancel restoring the snapshot
                */
            }
            return new ModuleAudio(schema, AudioDevices.AudioFormat, duration, captures.AsReadOnly());

            async Task<InstrumentAudio> RecordInstrument(Instrument instrument)
            {
                Progress.CurrentInstrumentRecording = $"Recording {instrument.Name}";

                device.Silence(Settings.SelectedMidiChannel);
                await device.SetInstrumentAsync(kitNumber, trigger: 1, instrument, token);
                var recordingTask = AudioDevices.RecordAudio(audioDeviceId, duration, token);
                device.PlayNote(midiChannel, midiNote, attack);
                var audio = await recordingTask;

                Progress.CompletedInstruments++;
                return new InstrumentAudio(instrument, audio);
            }

            int FindMidiNote(ModuleData kit)
            {
                // TODO: Don't assume this layout, and express it more cleanly.
                var triggerRoot = kit.LogicalRoot
                    .Children.First(n => n.SchemaNode.Name == "Triggers")
                    .Children.First(n => n.SchemaNode.Name == "Trigger[1]");
                return triggerRoot.GetMidiNote() ?? throw new InvalidOperationException($"Node {triggerRoot.SchemaNode.Path} has no MIDI note");
            }
        }
    }
}
