// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Test.Data.Fields;
internal class TempoDataFieldTest
{
    [Test]
    public void TrySetFormat()
    {
        var td27 = TestData.LoadTD27();
        var td27Schema = td27.Schema;

        var container = td27Schema.Kit1Root.Container.ResolveContainer("KitMfx[1]");
        var typeField = (EnumField) container.ResolveField("Type");
        var parametersField = (OverlayField) container.ResolveField("Parameters");
        
        var typeDataField = (EnumDataField) td27.Data.GetDataField(typeField);
        var overlayDataField = (OverlayDataField) td27.Data.GetDataField(parametersField);
        typeDataField.RawValue = 0; // Delay is the first MFX option.

        var delayLeftDataField = (TempoDataField) overlayDataField.CurrentFieldList.Fields[0];
        Assert.True(delayLeftDataField.TrySetFormattedText("Fixed: 6ms"));
        Assert.AreEqual("Fixed: 6ms", delayLeftDataField.FormattedText);
    }
}
