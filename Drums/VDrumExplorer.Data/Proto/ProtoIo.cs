// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;
using System.IO;
using System.Text;

namespace VDrumExplorer.Data.Proto
{
    internal static class ProtoIo
    {
        private const string MagicString = "JLSVDRUM1";
        private static readonly byte[] MagicBytes = Encoding.UTF8.GetBytes(MagicString);

        internal static void Write(Stream stream, Data.Module module) =>
            Write(stream, new DrumFile { Module = Module.FromModel(module) });

        internal static void Write(Stream stream, Data.Kit kit) =>
            Write(stream, new DrumFile { Kit = Kit.FromModel(kit) });

        internal static void Write(Stream stream, DrumFile drumFile)
        {
            stream.Write(MagicBytes, 0, MagicBytes.Length);
            drumFile.WriteTo(stream);
        }

        /// <summary>
        /// Returns the model from a stream.
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <returns>The model data. The type depends on the data in the stream.</returns>
        internal static object ReadStream(Stream stream)
        {
            var file = ReadDrumFile(stream);
            return file.FileCase switch
            {
                DrumFile.FileOneofCase.Kit => (object) file.Kit.ToModel(),
                DrumFile.FileOneofCase.Module => file.Module.ToModel(),
                _ => throw new InvalidDataException($"Unknown file case {file.FileCase}")
            };
        }

        private static DrumFile ReadDrumFile(Stream stream)
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
