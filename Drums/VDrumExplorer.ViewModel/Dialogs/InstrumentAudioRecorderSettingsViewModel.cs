// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.ViewModel.Dialogs
{
    public class InstrumentAudioRecorderSettingsViewModel : ViewModelBase
    {
        private readonly IViewServices viewServices;
        private readonly ModuleSchema schema;

        public InstrumentAudioRecorderSettingsViewModel(IViewServices viewServices, IAudioDeviceManager deviceManager, ModuleSchema schema, string midiName)
        {
            this.viewServices = viewServices;
            this.schema = schema;
            var groups = schema.InstrumentGroups
                .Where(ig => ig.Preset)
                .Select(ig => ig.Description)
                .ToList();
            groups.Insert(0, "(All)");
            InstrumentGroups = groups;
            selectedInstrumentGroup = groups[0];
            InputDevices = deviceManager.GetInputs();

            // Try to guess at a reasonable input based on known inputs including the MIDI name.
            var expectedInputDevices = new[] { $"MASTER ({midiName})", $"IN ({midiName})", $"KICK ({midiName})" };
            SelectedInputDevice = InputDevices.FirstOrDefault(input => expectedInputDevices.Contains(input.Name));

            kitNumber = schema.Kits;
            SelectOutputFileCommand = new DelegateCommand(SelectOutputFile, true);
        }

        private decimal recordingTime = 2.5m;
        public decimal RecordingTime
        {
            get => recordingTime;
            set
            {
                if (value < 0.1m)
                {
                    throw new ArgumentOutOfRangeException();
                }
                SetProperty(ref recordingTime, value);
            }
        }

        private int recordToPlayDelay = 20;
        public int RecordToPlayDelay
        {
            get => recordToPlayDelay;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                SetProperty(ref recordToPlayDelay, value);
            }
        }

        public IReadOnlyList<string> InstrumentGroups { get; }

        private string selectedInstrumentGroup;
        public string SelectedInstrumentGroup
        {
            get => selectedInstrumentGroup;
            set => SetProperty(ref selectedInstrumentGroup, value);
        }

        private int userSamples;
        public int UserSamples
        {
            get => userSamples;
            // Can't use ValidateUserSampleNumber, because 0 is fine here.
            set => SetProperty(ref userSamples, value, value >= 0 && value <= schema?.UserSampleInstruments.Count);
        }

        private IAudioInput? selectedInputDevice = null;
        public IAudioInput? SelectedInputDevice
        {
            get => selectedInputDevice;
            set => SetProperty(ref selectedInputDevice, value);
        }

        public IReadOnlyList<IAudioInput> InputDevices { get; }

        private int kitNumber;
        public int KitNumber
        {
            get => kitNumber;
            set => SetProperty(ref kitNumber, schema.ValidateKitNumber(value));
        }

        public IReadOnlyList<int> MidiChannels { get; } = Enumerable.Range(1, 16).ToList();

        private int selectedMidiChannel = 10;
        public int SelectedMidiChannel
        {
            get => selectedMidiChannel;
            // No validation, as we assume this is in a drop-down for now.
            set => SetProperty(ref selectedMidiChannel, value);
        }

        public int MinAttack => 1;
        public int MaxAttack => 127;

        private int attack = 80;
        public int Attack
        {
            get => attack;
            set => SetProperty(ref attack, value);
        }

        private string? outputFile = null;
        public string? OutputFile
        {
            get => outputFile;
            set => SetProperty(ref outputFile, value);
        }

        public ICommand SelectOutputFileCommand { get; }

        private void SelectOutputFile()
        {
            var file = viewServices.ShowSaveFileDialog(FileFilters.InstrumentAudioFiles);
            if (file != null)
            {
                OutputFile = file;
            }
        }
    }
}
