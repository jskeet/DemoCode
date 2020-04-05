// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;

namespace VDrumExplorer.Proto
{
    /// <summary>
    /// Extension methods on models, allowing simple client code without the model
    /// needing to know about protos.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Saves a module to the specified stream.
        /// </summary>
        /// <param name="module">The module to save. Must not be null.</param>
        /// <param name="stream">The stream to save the module to. Must not be null.</param>
        public static void Save(this Model.Module module, Stream stream) => ProtoIo.Write(stream, module);

        /// <summary>
        /// Saves a kit to the specified stream.
        /// </summary>
        /// <param name="kit">The kit to save. Must not be null.</param>
        /// <param name="stream">The stream to save the module to. Must not be null.</param>
        public static void Save(this Model.Kit kit, Stream stream) => ProtoIo.Write(stream, kit);

        /// <summary>
        /// Saves module audio data to the specified stream. 
        /// </summary>
        /// <param name="audio">The audio data to save. Must not be null.</param>
        /// <param name="stream">The stream to save the data to. Must not be null.</param>
        public static void Save(this Model.Audio.ModuleAudio audio, Stream stream) => ProtoIo.Write(stream, audio);
    }
}
