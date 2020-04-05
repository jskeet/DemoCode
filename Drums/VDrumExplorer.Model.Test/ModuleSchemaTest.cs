// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using VDrumExplorer.Model.Fields;
using VDrumExplorer.Proto;

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

        // We possibly want a whole project for integration type tests...
        [Test]
        public void StringDataSmokeTest()
        {
            Module module;
            using (var stream = typeof(ModuleSchemaTest).Assembly.GetManifestResourceStream("td27.vdrum"))
            {
                module = (Module) ProtoIo.ReadModel(stream);
            }
            var (fieldContainer, field) = module.Schema.PhysicalRoot.ResolveField("/Kit[12]/KitCommon/KitName");
            var data = module.Data.GetContainerData(fieldContainer);
            var stringField = (StringField) field;
            var value = stringField.GetText(data);
            // Note that the model does not perform text trimming: it provides the text exactly as in the data.
            Assert.AreEqual("Studio B    ", value);
        }
    }
}
