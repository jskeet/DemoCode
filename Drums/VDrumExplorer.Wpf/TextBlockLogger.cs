// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Windows.Controls;

namespace VDrumExplorer.Wpf
{
    internal class TextBlockLogger : ILogger
    {
        private readonly TextBlock block;

        internal TextBlockLogger(TextBlock block) =>
            this.block = block;

        public void Log(string text) =>
            block.Text += $"{DateTime.Now:HH:mm:ss.fff} {text}\r\n";

        public void Log(string message, Exception e)
        {
            // TODO: Aggregate exception etc.
            Log($"{message}: {e}");
        }

        public void SaveLog(string file) => File.WriteAllText(file, block.Text);
    }
}
