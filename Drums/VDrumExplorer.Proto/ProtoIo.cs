// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Proto
{
    /// <summary>
    /// I/O methods for Protocol Buffer representations of V-Drum data.
    /// </summary>
    public static class ProtoIo
    {
        private const string MagicString = "JLSVDRUM1";
        private static readonly byte[] MagicBytes = Encoding.UTF8.GetBytes(MagicString);

        /// <summary>
        /// Creates a model from the data in a stream.
        /// </summary>
        /// <param name="stream">The stream to read data from.</param>
        /// <param name="validationResult">The result of validating the data.</param>
        /// <returns>The model data. The type depends on the data in the stream.</returns>
        public static object ReadModel(Stream stream, ILogger logger)
        {
            var file = ReadDrumFile(stream);
            return file.FileCase switch
            {
                DrumFile.FileOneofCase.Kit => (object) file.Kit.ToModel(logger),
                DrumFile.FileOneofCase.Module => file.Module.ToModel(logger),
                DrumFile.FileOneofCase.ModuleAudio => file.ModuleAudio.ToModel(logger),
                _ => throw new InvalidDataException($"Unknown file case {file.FileCase}")
            };
        }

        /// <summary>
        /// Loads a model from the data in a file. This is a convenience method to call
        /// <see cref="ReadModel(Stream)"/> using a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="logger">The logger to write validation results to.</param>
        /// <returns>The model data. The type depends on the data in the stream.</returns>
        public static object LoadModel(string file, ILogger logger)
        {
            using (var stream = File.OpenRead(file))
            {
                return ReadModel(stream, logger);
            }
        }

        // Note: these methods are currently called by the convenience methods in ModelExtensions.
        // We could make them public if we wanted.
        internal static void Write(Stream stream, Model.Module module) =>
            Write(stream, new DrumFile { Module = Module.FromModel(module) });

        internal static void Write(Stream stream, Model.Kit kit) =>
            Write(stream, new DrumFile { Kit = Kit.FromModel(kit) });

        internal static void Write(Stream stream, Model.Audio.ModuleAudio audio) =>
            Write(stream, new DrumFile { ModuleAudio = ModuleAudio.FromModel(audio) });

        internal static void Write(Stream stream, DrumFile drumFile)
        {
            stream.Write(MagicBytes, 0, MagicBytes.Length);
            drumFile.WriteTo(stream);
        }

        internal static DrumFile ReadDrumFile(Stream stream)
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
