// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;

namespace VDrumExplorer.ViewModel.Dialogs
{
    public class InstrumentAudioRecorderViewModel : ViewModelBase
    {
        private CancellationTokenSource? cancellationTokenSource;
        private readonly ModuleSchema schema;
        private readonly RolandMidiClient device;
        private ILogger logger;
        public InstrumentAudioRecorderSettingsViewModel Settings { get; }
        public InstrumentAudioRecorderProgressViewModel Progress { get; }
        public CommandBase StartRecordingCommand { get; }
        public CommandBase CancelCommand { get; }
        public string Title { get; }

        public InstrumentAudioRecorderViewModel(IViewServices viewServices, ILogger logger, DeviceViewModel deviceViewModel)
        {
            this.logger = logger;
            schema = deviceViewModel.ConnectedDeviceSchema ?? throw new InvalidOperationException("Cannot record audio without a connected device");
            device = deviceViewModel.ConnectedDevice ?? throw new InvalidOperationException("Cannot record audio without a connected device");

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
            // Need to:
            // - Find the logical node for the instrument within the kit
            // - Work out the MIDI note for kick
            // 

            var originalKit = await GetCurrentKit();
            logger.LogInformation($"Changing from kit {originalKit} to {Settings.KitNumber}");
            await SetCurrentKit(Settings.KitNumber);
            try
            {
                await RecordInstruments(token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"Recording operation canceled");
                Progress.CurrentInstrumentRecording = "Recording cancelled";
            }
            catch (Exception e)
            {
                logger.LogError($"Recording operation failed", e);
                Progress.CurrentInstrumentRecording = "Error - see log";
            }
            finally
            {
                logger.LogInformation($"Changing kit back to {originalKit}");
                await SetCurrentKit(originalKit);
            }

            /*

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

            var presetInstrumentsToRecord = schema.PresetInstruments
                .Where(ins => config.InstrumentGroup == -1 || ins.Group.Index == config.InstrumentGroup)
                .ToList();
            progress.Maximum = presetInstrumentsToRecord.Count + config.UserSamples;

            logger.Log($"Starting recording process");
            try
            {
                var captures = new List<InstrumentAudio>();
                progress.Value = 0;
                foreach (var instrument in presetInstrumentsToRecord)
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
                // Note: setting the instrument resets VEdit data to defaults
                instrumentField.SetInstrument(instrumentFieldContext, data, instrument);
                foreach (var container in instrumentContainers)
                {
                    var segment = data.GetSegment(container.Address);
                    midiClient.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40, CancellationToken);
                }
                midiClient.Silence(config.MidiChannel);
                await Task.Delay(40);
                var recordingTask = AudioDevices.RecordAudio(config.AudioDeviceId, config.RecordingDuration, CancellationToken);
                midiClient.PlayNote(config.MidiChannel, midiNote.Value, config.Attack);
                var audio = await recordingTask;
                return new InstrumentAudio(instrument, audio);
            }*/
        }
        /*
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
            var segment = await device.RequestDataAsync(context.Address.Value, context.Container.Size, effectiveToken);
            data.Populate(context.Address, segment);
        }*/

        private async Task RecordInstruments(CancellationToken token)
        {
            var selectedInstrumentGroup = schema.InstrumentGroups.FirstOrDefault(ig => ig.Description == Settings.SelectedInstrumentGroup);
            var instrumentsToRecord = schema.PresetInstruments.Where(inst => selectedInstrumentGroup is null || inst.Group == selectedInstrumentGroup)
                .Concat(schema.UserSampleInstruments.Take(Settings.UserSamples))
                .ToList();

            Progress.TotalInstruments = instrumentsToRecord.Count;
            Progress.CompletedInstruments = 0;

            // Load the details for the whole kit.
            // We don't need all of it, but it doesn't take *that* long, compared with the rest of the process.
            var schemaKitRoot = schema.KitRoots[Settings.KitNumber - 1];
            var moduleData = ModuleData.FromLogicalRootNode(schemaKitRoot);
            var dataKitRoot = moduleData.LogicalRoot;
            // TODO: Don't assume this layout, and express it more cleanly.
            var triggerRoot = dataKitRoot.Children.First(n => n.SchemaNode.Name == "Triggers")
                .Children.First(n => n.SchemaNode.Name == "Trigger[1]");

            Progress.CurrentInstrumentRecording = "Loading kit data";
            logger.LogInformation($"Loading data from kit {Settings.KitNumber} to restore later");
            var snapshot = await LoadSnapshotFromDevice(moduleData, token);
            // Populate the module data with the snapshot we've created.
            moduleData.LoadSnapshot(snapshot);
            var midiNote = triggerRoot.GetMidiNote() ?? throw new InvalidOperationException($"Node {triggerRoot.SchemaNode.Path} has no MIDI note");

            // TODO: Reset everything on the kit (because of mfx, vedit etc)
            var instrumentFields = triggerRoot.Details
                .OfType<FieldContainerDataNodeDetail>()
                .SelectMany(child => child.Fields.OfType<InstrumentDataField>())
                .ToList();
            if (instrumentFields.Count != 2)
            {
                throw new InvalidOperationException("Expected to find two instrument fields: main and sub");
            }
            // TODO: How do we save a FieldContainerDataNodeDetail to a DataSegment? New method?
            // TODO: Can we just poke the instrument field itself? (Well, the two bits of it...)

            logger.LogInformation($"Recording {instrumentsToRecord.Count} instruments");
            try
            {
                // Just simulate it for the moment...
                for (int i = 0; i < instrumentsToRecord.Count; i++)
                {
                    Progress.CurrentInstrumentRecording = instrumentsToRecord[i].Name;
                    await Task.Delay(100, token);
                    Progress.CompletedInstruments = i + 1;
                }
            }
            finally
            {
                Progress.CurrentInstrumentRecording = "Restoring kit data";
                logger.LogInformation($"Restoring snapshot to kit {Settings.KitNumber}");
                // Don't cancel restoring the snapshot
                await SaveSnapshotToDevice(snapshot, CancellationToken.None);
            }
            logger.LogInformation($"Recording complete");
            Progress.CurrentInstrumentRecording = "Complete";
        }

        // TODO: Put these somewhere common.
        private async Task SetCurrentKit(int newKitNumber)
        {
            device.SendData(0, new[] { (byte) (newKitNumber - 1) });
            await Task.Delay(40);
        }

        private async Task<int> GetCurrentKit()
        {
            // TODO: Stop assuming current kit is at address 0, although it is for TD-17, TD-50 and TD-27...
            var data = await device.RequestDataAsync(0, 1, new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);
            return data[0] + 1;
        }

        private async Task<ModuleDataSnapshot> LoadSnapshotFromDevice(ModuleData moduleData, CancellationToken token)
        {
            var snapshot = new ModuleDataSnapshot();
            // TODO: Make it easier to find what we need to load/save.
            foreach (var segment in moduleData.CreateSnapshot().Segments)
            {
                snapshot.Add(await LoadSegment(segment.Address, segment.Size, token));
            }
            return snapshot;
        }

        private async Task SaveSnapshotToDevice(ModuleDataSnapshot snapshot, CancellationToken token)
        {
            foreach (var segment in snapshot.Segments)
            {
                device.SendData(segment.Address.DisplayValue, segment.CopyData());
                await Task.Delay(40, token);
            }
        }

        private async Task<DataSegment> LoadSegment(ModuleAddress address, int size, CancellationToken token)
        {
            var timerToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(token, timerToken).Token;
            var data = await device.RequestDataAsync(address.DisplayValue, size, effectiveToken);
            return new DataSegment(address, data);
        }
    }
}
