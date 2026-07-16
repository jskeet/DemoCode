// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            MidiDevices.Manager = new Midi.ManagedMidi.MidiManager();

            var rootCommand = new RootCommand
            {
                Description = "V-Drum Explorer console interface"
            };
            rootCommand.Add(ListDevicesCommand.Command);
            rootCommand.Add(ListKitsCommand.Command);
            rootCommand.Add(ImportKitCommand.Command);
            rootCommand.Add(ShowKitCommand.Command);
            rootCommand.Add(SendMidiEventsCommand.Command);
            rootCommand.Add(ShowMidiEventsCommand.Command);
            rootCommand.Add(ShowSchemaStatsCommand.Command);
            rootCommand.Add(TurnPagesViaKitChangeCommand.Command);
            rootCommand.Add(TurnPagesViaMidiCommand.Command);
            rootCommand.Add(DumpAerophoneMiniCommand.Command);
            rootCommand.Add(DumpDataCommand.Command);
            rootCommand.Add(DumpProtoCommand.Command);
            rootCommand.Add(DumpAllDataCommand.Command);
            rootCommand.Add(ProxyMidiCommand.Command);
            rootCommand.Add(ListAerophoneStudioSets.Command);
            rootCommand.Add(CopyAerophoneStudioSets.Command);
            rootCommand.Add(ConvertToJsonCommand.Command);
#if NETCOREAPP3_1_OR_GREATER
            rootCommand.Add(CheckInstrumentDefaultsCommand.Command);
            rootCommand.Add(CheckMfxDefaultsCommand.Command);
            rootCommand.Add(DumpDeviceSegmentCommand.Command);
#endif
            ParseResult parseResult = rootCommand.Parse(args);
            return parseResult.InvokeAsync();
        }
    }
}
