// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

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

        // Currently just validates overlay fields (inefficiently, at that...)
        // Better than nothing though.
        // TODO: Have a way of getting at "just the field containers without variables".
        [Test]
        [TestCaseSource(nameof(KnownSchemaIdentifiers))]
        public void SchemasAreValid(ModuleIdentifier id)
        {
            var schema = ModuleSchema.KnownSchemas[id].Value;
            IReadOnlyList<string> veditCategories = schema.InstrumentGroups.Select(g => g.VEditCategory).ToList();
            foreach (var container in schema.PhysicalRoot.DescendantsAndSelf().OfType<FieldContainer>())
            {
                foreach (var field in container.Fields.OfType<OverlayField>())
                {
                    var validValues = field.FieldLists.Keys;
                    var switchField = container.ResolveField(field.SwitchPath);
                    var possibleValues = switchField switch
                    {
                        EnumField enumField => enumField.Values,
                        InstrumentField instrumentField => veditCategories,
                        _ => throw new AssertionException($"Unexpected switch field type {switchField.GetType()} for field {field.Path}")
                    };
                    var invalid = possibleValues.Except(validValues).ToList();
                    Assert.IsEmpty(invalid, "Invalid values for switch field {0}", field.Path);
                }
            }
        }
    }
}
