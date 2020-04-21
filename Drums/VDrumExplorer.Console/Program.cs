﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.Threading.Tasks;

namespace VDrumExplorer.Console
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "V-Drum Explorer console interface"
            };
            rootCommand.AddCommand(ListDevicesCommand.Command);
            rootCommand.AddCommand(ImportKitCommand.Command);
            rootCommand.AddCommand(ShowKitCommand.Command);
            rootCommand.AddCommand(CheckInstrumentDefaultsCommand.Command);
            rootCommand.AddCommand(ShowMidiEventsCommand.Command);
            rootCommand.AddCommand(ShowSchemaStatsCommand.Command);
            rootCommand.AddCommand(TurnPagesViaKitChangeCommand.Command);
            rootCommand.AddCommand(TurnPagesViaMidiCommand.Command);
            return rootCommand.InvokeAsync(args);
        }
    }
}
