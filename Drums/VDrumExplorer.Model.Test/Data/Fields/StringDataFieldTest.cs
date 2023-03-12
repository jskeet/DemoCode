// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System;
using System.Text;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.Model.Test.Data.Fields;

public class StringDataFieldTest
{
    private StringDataField CreateField()
    {
        Module module = TestData.LoadTD27();
        var field = module.Schema.PhysicalRoot.ResolveField("/Kit[12]/KitCommon/KitName");
        return (StringDataField) module.Data.GetDataField(field);
    }

    [Test]
    public void GetText()
    {
        var field = CreateField();
        Assert.AreEqual("Studio B", field.Text);
    }

    [Test]
    public void SetText_TriggersChange()
    {
        var field = CreateField();
        var recorder = new NotifyChangeRecorder(field);      
        field.Text = "Some text";
        Assert.AreEqual(new[] { nameof(field.FormattedText) }, recorder.ChangedProperties);
    }

    [Test]
    public void DataChange_TriggersChange()
    {
        var field = CreateField();
        var recorder = new NotifyChangeRecorder(field);

        // Validate that it's a 1-byte-per-char field
        Assert.AreEqual(field.SchemaField.Length, field.SchemaField.Size);

        var container = field.SchemaField.Parent!;
        var segment = new DataSegment(container.Address, new byte[container.Size]);
        field.Save(segment);
        var bytes = Encoding.ASCII.GetBytes("abc");
        segment.WriteBytes(field.SchemaField.Offset, bytes.AsSpan());
        field.Load(segment);

        CollectionAssert.AreEqual(new[] { nameof(field.FormattedText) }, recorder.ChangedProperties);
        Assert.AreEqual("abcdio B", field.Text);
    }

    [Test]
    public void DataChange_DoesNotTriggerChangeIfValueStaysTheSame()
    {
        var field = CreateField();
        var recorder = new NotifyChangeRecorder(field);

        // Validate that it's a 1-byte-per-char field
        Assert.AreEqual(field.SchemaField.Length, field.SchemaField.Size);

        var container = field.SchemaField.Parent!;
        var segment = new DataSegment(container.Address, new byte[container.Size]);
        field.Save(segment);
        var bytes = Encoding.ASCII.GetBytes("Studio B");
        segment.WriteBytes(field.SchemaField.Offset, bytes.AsSpan());
        field.Load(segment);

        // The data in the field wasn't modified, so we didn't report a change.
        Assert.IsEmpty(recorder.ChangedProperties);
    }

    [Test]
    public void DataChangeElsewhere_DoesNotTriggerChange()
    {
        var field = CreateField();
        var recorder = new NotifyChangeRecorder(field);

        var container = field.SchemaField.Parent!;
        var otherField = container.ResolveField("KitSubName");
        var segment = new DataSegment(container.Address, new byte[container.Size]);
        field.Save(segment);
        var bytes = Encoding.ASCII.GetBytes("Studio");
        segment.WriteBytes(otherField.Offset, bytes.AsSpan());
        field.Load(segment);

        // The data in the field we're watching wasn't modified, so we didn't report a change.
        Assert.IsEmpty(recorder.ChangedProperties);
    }
}
