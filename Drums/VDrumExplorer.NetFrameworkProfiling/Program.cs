// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics;
using System.Linq;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Proto;
using VDrumExplorer.Utility;

namespace VDrumExplorer.NetFrameworkProfiling
{
    /// <summary>
    /// For whatever reason, I've found it easiest to perform profiling
    /// (e.g. with CodeTrack - https://www.getcodetrack.com/) via .NET Framework
    /// console apps than with .NET Core. This project is expected to change
    /// purpose several times, but should just do one thing that's easy to profile
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string file = args[0];
            for (int i = 0; i < 10; i++)
            {
                var model = (Module) Timing.DebugConsoleLogTiming("Loaded model", () => ProtoIo.LoadModel(file));

                var containers = model.Schema.PhysicalRoot.DescendantsAndSelf().OfType<FieldContainer>().ToList();
                Timing.DebugConsoleLogTiming("Populated dictionaries", () => containers.ForEach(fc => fc.GetFieldOrNull("".AsSpan())));
            }
        }
    }
}
