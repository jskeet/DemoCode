// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.ViewModel.Audio
{
    public class InstrumentAudioExplorerViewModel : ViewModelBase<ModuleAudio>
    {
        public string Title { get; }

        public IReadOnlyList<IAudioOutput> OutputDevices { get; }
        public string ModuleName => Model.Schema.Identifier.Name;
        public string AudioFormat { get; }

        private IAudioOutput? selectedOutputDevice;
        public IAudioOutput? SelectedOutputDevice
        {
            get => selectedOutputDevice;
            set => SetProperty(ref selectedOutputDevice, value);
        }

        public IReadOnlyList<InstrumentGroupAudioViewModel> Groups { get; }

        private InstrumentGroupAudioViewModel? selectedGroup;
        public InstrumentGroupAudioViewModel? SelectedGroup
        {
            get => selectedGroup;
            set
            {
                if (SetProperty(ref selectedGroup, value))
                {
                    // Ideally, we'd select the first item to get the scrollbar
                    // to go to the top... but that alos plays the first sound.
                    SelectedAudio = null;
                }
            }
        }

        public double DurationSeconds => Model.DurationPerInstrument.TotalSeconds;
        public int UserSampleCount { get; }


        private InstrumentAudio? selectedAudio;
        public InstrumentAudio? SelectedAudio
        {
            get => selectedAudio;
            set => SetProperty(ref selectedAudio, value);
        }

        public InstrumentAudioExplorerViewModel(IAudioDeviceManager deviceManager, ModuleAudio model, string? file) : base(model)
        {
            Title = file is null
                ? $"Instrument Audio Explorer ({model.Schema.Identifier.Name})"
                : $"Instrument Audio Explorer ({model.Schema.Identifier.Name}) - {file}";

            var format = model.Format;
            AudioFormat = $"Channels: {format.Channels}; Bits: {format.Bits}; Frequency: {format.Frequency}";
            OutputDevices = deviceManager.GetOutputs();
            selectedOutputDevice = OutputDevices.FirstOrDefault();

            Groups = model.Captures
                .GroupBy(ia => ia.Instrument.Group)
                .Select(InstrumentGroupAudioViewModel.FromGrouping)
                .ToList();
            SelectedGroup = Groups.FirstOrDefault();
            UserSampleCount = Groups.FirstOrDefault(vm => !vm.Group.Preset)?.Audio.Count ?? 0;
        }

        public async Task PlayAudio()
        {
            if (SelectedOutputDevice is null || SelectedAudio is null)
            {
                return;
            }
            await SelectedOutputDevice.PlayAudioAsync(Model.Format, SelectedAudio.Audio, CancellationToken.None);
        }
    }
}
