// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.ViewModel.Dialogs
{
    public class InstrumentAudioRecorderProgressViewModel : ViewModelBase
    {
        private string? currentInstrumentRecording = "Progress";
        public string? CurrentInstrumentRecording
        {
            get => currentInstrumentRecording;
            internal set => SetProperty(ref currentInstrumentRecording, value);
        }

        private int totalInstruments;
        public int TotalInstruments
        {
            get => totalInstruments;
            internal set => SetProperty(ref totalInstruments, value);
        }

        private int completedInstruments;
        public int CompletedInstruments
        {
            get => completedInstruments;
            internal set => SetProperty(ref completedInstruments, value);
        }
    }
}
