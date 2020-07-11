// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Model.Test
{
    public class ModuleSchemaTest
    {
        private static readonly List<ModuleIdentifier> KnownSchemaIdentifiers = ModuleSchema.KnownSchemas.Keys.ToList();

        [Test]
        [TestCaseSource(nameof(KnownSchemaIdentifiers))]
        public void KnownSchemasLoadWithCorrectIdentifier(ModuleIdentifier id)
        {
            var schema = ModuleSchema.KnownSchemas[id].Value;
            Assert.AreEqual(id, schema.Identifier);
        }
    }
}
