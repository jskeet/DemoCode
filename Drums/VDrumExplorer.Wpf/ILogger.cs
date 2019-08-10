// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Wpf
{
    internal interface ILogger
    {
        void Log(string text);
        void Log(string message, Exception e);
    }
}
