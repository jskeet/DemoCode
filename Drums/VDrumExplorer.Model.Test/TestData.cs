// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using VDrumExplorer.Proto;

namespace VDrumExplorer.Model.Test
{
    internal class TestData
    {
        // TODO: Once we have ModuleData cloning, we can just load once.
        public static Module LoadTD27()
        {
            using (var stream = typeof(ModuleSchemaTest).Assembly.GetManifestResourceStream("td27.vdrum"))
            {
                // TODO: Validate that there are no validation errors via a logger.
                return (Module) ProtoIo.ReadModel(stream, NullLogger.Instance);
            }
        }
    }
}
