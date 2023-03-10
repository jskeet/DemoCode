// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;

namespace VDrumExplorer.Model.Json
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
        /// <param name="writer">The writer to save the module to. Must not be null.</param>
        public static void SaveAsJson(this Module module, TextWriter writer) => JsonIo.Write(writer, module);

        /// <summary>
        /// Saves a kit to the specified stream.
        /// </summary>
        /// <param name="kit">The kit to save. Must not be null.</param>
        /// <param name="writer">The writer to save the module to. Must not be null.</param>
        public static void SaveAsJson(this Kit kit, TextWriter writer) => JsonIo.Write(writer, kit);

        /// <summary>
        /// Converts a module to JSON. This is a convenience method over <see cref="SaveAsJson(Module, TextWriter)"/>.
        /// </summary>
        public static string ToJson(this Module module)
        {
            using var writer = new StringWriter();
            module.SaveAsJson(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Converts a kit to JSON. This is a convenience method over <see cref="SaveAsJson(Kit, TextWriter)"/>.
        /// </summary>
        public static string ToJson(this Kit kit)
        {
            using var writer = new StringWriter();
            kit.SaveAsJson(writer);
            return writer.ToString();
        }
    }
}
