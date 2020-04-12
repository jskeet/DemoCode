// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;

namespace VDrumExplorer.Model.Test
{
    public class ModuleSchemaTest
    {
        [Test]
        public void KnownSchemasLoadWithCorrectIdentifier()
        {
            foreach (var pair in ModuleSchema.KnownSchemas)
            {
                var schema = pair.Value.Value;
                Assert.AreEqual(pair.Key, schema.Identifier);
            }
        }
    }
}
