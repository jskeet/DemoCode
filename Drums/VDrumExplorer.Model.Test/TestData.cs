// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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
                return (Module) ProtoIo.ReadModel(stream);
            }
        }
    }
}
