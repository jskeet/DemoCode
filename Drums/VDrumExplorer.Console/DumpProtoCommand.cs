// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using VDrumExplorer.Proto;

namespace VDrumExplorer.Console
{
    internal sealed class DumpProtoCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("dump-proto")
        {
            Description = "Loads a V-Drum Explorer file, and writes it to the console (primarily for diffing)",
            Handler = new DumpProtoCommand()
        }
        .AddRequiredOption<string>("--file", "File to load");

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var file = context.ParseResult.ValueForOption<string>("file");
            DrumFile proto;
            using (var stream = File.OpenRead(file))
            {
                proto = ProtoIo.ReadDrumFile(stream);
            }
            switch (proto.FileCase)
            {
                case DrumFile.FileOneofCase.Module:
                    DumpModule(proto.Module);
                    break;
                case DrumFile.FileOneofCase.Kit:
                    DumpKit(proto.Kit);
                    break;
                case DrumFile.FileOneofCase.ModuleAudio:
                    DumpModuleAudio(proto.ModuleAudio);
                    break;
            }
            return Task.FromResult(0);


            void DumpModule(Module module)
            {
                console.WriteLine($"File type: module");
                DumpIdentifier(module.Identifier);
                console.WriteLine("Data segments:");
                DumpDataSegments(module.Containers);
            }

            void DumpKit(Kit kit)
            {
                console.WriteLine($"File type: kit");
                DumpIdentifier(kit.Identifier);
                console.WriteLine($"Kit number: {kit.DefaultKitNumber}");
                console.WriteLine("Data segments:");
                DumpDataSegments(kit.Containers);
            }

            void DumpModuleAudio(ModuleAudio moduleAudio)
            {
                console.WriteLine($"File type: module audio");
                DumpIdentifier(moduleAudio.Identifier);
                console.WriteLine($"Format: {moduleAudio.Format}");
                console.WriteLine($"Duration per instrument: {moduleAudio.DurationPerInstrument}");
                foreach (var capture in moduleAudio.InstrumentCaptures)
                {
                    console.WriteLine($"{(capture.Preset ? "Preset" : "Sample")} {capture.InstrumentId}: {capture.AudioData}");
                }
            }
            
            void DumpIdentifier(ModuleIdentifier identifier)
            {
                // JSON representation is fine here.
                console.WriteLine($"Identifier: {identifier}");
            }

            void DumpDataSegments(IEnumerable<FieldContainerData> segments)
            {
                foreach (var segment in segments)
                {
                    console.WriteLine($"{segment.Address:x8}: {BitConverter.ToString(segment.Data.ToByteArray())}");
                }
            }
        }
    }
}
