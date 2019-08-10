using System;

namespace VDrumExplorer.Wpf
{
    internal interface ILogger
    {
        void Log(string text);
        void Log(string message, Exception e);
    }
}
