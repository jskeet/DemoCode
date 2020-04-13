﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;
using ModuleIdentifier = VDrumExplorer.Midi.ModuleIdentifier;
using VDrumExplorer.Data.Proto;

namespace VDrumExplorer.Data
{
    // TODO: Not really sure what to do with this, static/instance methods, non-public etc.
    public static class SchemaRegistry
    {
        public static IReadOnlyDictionary<ModuleIdentifier, Lazy<ModuleSchema>> KnownSchemas { get; }
            = new Dictionary<ModuleIdentifier, Lazy<ModuleSchema>>
        {
            { ModuleIdentifier.TD17, CreateLazySchema("VDrumExplorer.Data.TD17", "TD17.json") },
            { ModuleIdentifier.TD27, CreateLazySchema("VDrumExplorer.Data.TD27", "TD27.json") },
            { ModuleIdentifier.TD50, CreateLazySchema("VDrumExplorer.Data.TD50", "TD50.json") }

        }.AsReadOnly();
        
        private static Lazy<ModuleSchema> CreateLazySchema(string resourceBase, string resourceName) =>
            new Lazy<ModuleSchema>(() => ModuleSchema.FromAssemblyResources(typeof(SchemaRegistry).Assembly, resourceBase, resourceName));

        // It's unpleasant having this here, but I'm not sure of the alternative...
        /// <summary>
        /// Reads the given stream as drum data, returning a Kit or a Module depending
        /// on the data within the stream.
        /// </summary>
        public static object ReadStream(Stream stream) => ProtoIo.ReadStream(stream);
    }
}
