// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Data.Proto
{
    internal static class ProtoIo
    {
        private const string MagicString = "JLSVDRUM1";
        private static readonly byte[] MagicBytes = Encoding.UTF8.GetBytes(MagicString);

        internal static void Write(Stream stream, Kit kit)
        {
        }

        internal static void Write(Stream stream, Data.Module module)
        {
            var protoModule = new Module
            {
                Identifier = ModuleIdentifier.FromModel(module.Schema.Identifier),
                Segments = { module.Data.GetSegments().Select(DataSegment.FromModel) }
            };
            Write(stream, new DrumFile { Module = protoModule });
        }

        internal static void Write(Stream stream, DrumFile drumFile)
        {
            stream.Write(MagicBytes, 0, MagicBytes.Length);
            drumFile.WriteTo(stream);
        }

        internal static Data.Module ReadModule(Stream stream)
        {
            var file = ReadFile(stream);
            if (file.FileCase != DrumFile.FileOneofCase.Module)
            {
                throw new InvalidDataException($"Expected a module; actual case = {file.FileCase}");
            }
            var protoModule = file.Module;
            var identifier = protoModule.Identifier.ToModel();
            if (!SchemaRegistry.KnownSchemas.TryGetValue(identifier, out var schema))
            {
                throw new InvalidDataException($"No known schema matches identifier {identifier}");
            }
            var moduleData = new ModuleData();
            foreach (var segment in protoModule.Segments)
            {
                moduleData.Populate(new ModuleAddress(segment.Start), segment.Data.ToByteArray());
            }
            return new Data.Module(schema.Value, moduleData);
        }

        private static DrumFile ReadFile(Stream stream)
        {
            // Simpler than using bulk read...
            for (int i = 0; i < MagicBytes.Length; i++)
            {
                int streamByte = stream.ReadByte();
                if (streamByte == -1)
                {
                    throw new EndOfStreamException("Magic number missing from stream");
                }
                if (streamByte != MagicBytes[i])
                {
                    throw new InvalidDataException($"Magic number invalid in stream. Index={i}; Expected={MagicBytes[i]}; Actual={streamByte}");
                }
            }

            return DrumFile.Parser.ParseFrom(stream);
        }        
    }
}
