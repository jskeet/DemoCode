// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Centralized repository of file filters for save/open file dialog boxes
    /// </summary>
    internal static class FileFilters
    {
        internal const string KitFiles = "Kit files|*.vkit";
        internal const string ModuleFiles = "V-Drum Explorer module files|*.vdrum";
        internal const string InstrumentAudioFiles = "V-Drum Explorer audio files|*.vaudio";
        internal const string AllExplorerFiles = "All explorer files|*.vdrum;*.vkit;*.vaudio|Module files|*.vdrum|Kit files|*.vkit|Audio files|*.vaudio";
        internal const string LogFiles = "Log files|*.json";
    }
}
