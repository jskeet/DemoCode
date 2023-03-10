// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Test.Schema.Fields;

internal class NumericFieldTest
{
    private static readonly List<ModuleIdentifier> KnownSchemaIdentifiers = ModuleSchema.KnownSchemas.Keys.ToList();

    /// <summary>
    /// Round trip all values in all numeric fields in all schemas.
    /// This is very repetitive (there'll be many fields which are the same) but it means
    /// that any lurking corner cases are handled.
    /// </summary>
    [Test]
    [TestCaseSource(nameof(KnownSchemaIdentifiers))]
    public void ExhaustiveRoundTrip(ModuleIdentifier id)
    {
        var schema = ModuleSchema.KnownSchemas[id].Value;
        var fields = schema.PhysicalRoot.DescendantsAndSelf()
            .OfType<FieldContainer>()
            .SelectMany(fc => fc.Fields);
        var allOverlayFields = fields.OfType<OverlayField>().SelectMany(overlay => overlay.FieldLists.Values.SelectMany(fl => fl.Fields)).Distinct();

        foreach (var field in fields.Concat(allOverlayFields).OfType<NumericField>())
        {
            for (int i = field.Min; i <= field.Max; i++)
            {
                var text = field.FormatRawValue(i);
                int? result = field.TryParseFormattedValue(text);
                Assert.AreEqual(i, result);
            }
        }
    }
}