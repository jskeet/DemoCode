// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Device
{
    /// <summary>
    /// Indication of the progress when transferring data.
    /// </summary>
    public sealed class TransferProgress
    {
        public int Completed { get; }
        public int Total { get; }
        public string Current { get; }

        public TransferProgress(int completed, int total, string current) =>
            (Completed, Total, Current) = (completed, total, current);
    }
}
