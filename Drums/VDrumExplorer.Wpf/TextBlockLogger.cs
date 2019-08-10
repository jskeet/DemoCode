using System;
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
    }
}
