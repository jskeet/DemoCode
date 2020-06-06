// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.Model.Test.Data.Fields
{
    public class StringDataFieldTest
    {
        private StringDataField CreateField()
        {
            Module module = TestData.LoadTD27();
            var field = module.Schema.PhysicalRoot.ResolveField("/Kit[12]/KitCommon/KitName");
            return (StringDataField) module.Data.GetDataField(field);
        }

        /*
        [Test]
        public void GetText()
        {
            var field = CreateField();
            Assert.AreEqual("Studio B    ", field.Text);
        }

        [Test]
        public void SetText_TriggersChange()
        {
            var field = CreateField();
            var recorder = new NotifyChangeRecorder(field);            
            field.Text = "Some text";
            Assert.AreEqual(new[] { nameof(field.Text) }, recorder.ChangedProperties);
        }

        [Test]
        public void SetText_TriggersChange_EvenIfValueStaysTheSame()
        {
            var field = CreateField();
            var recorder = new NotifyChangeRecorder(field);
            field.Text = field.Text;
            Assert.AreEqual(new[] { nameof(field.Text) }, recorder.ChangedProperties);
        }

        [Test]
        public void DataChange_TriggersChange()
        {
            var field = CreateField();
            var context = field.Context;
            var recorder = new NotifyChangeRecorder(field);

            // Validate that it's a 1-byte-per-char field
            Assert.AreEqual(field.SchemaField.Length, field.SchemaField.Size);

            var bytes = Encoding.ASCII.GetBytes("abc");
            context.WriteBytes(field.SchemaField.Offset, bytes.AsSpan());

            CollectionAssert.AreEqual(new[] { nameof(field.Text) }, recorder.ChangedProperties);
            Assert.AreEqual("abcdio B    ", field.Text);
        }

        [Test]
        public void DataChange_TriggersChange_EvenIfValueStaysTheSame()
        {
            var field = CreateField();
            var context = field.Context;
            var recorder = new NotifyChangeRecorder(field);

            // Validate that it's a 1-byte-per-char field
            Assert.AreEqual(field.SchemaField.Length, field.SchemaField.Size);

            var originalValue = field.Text;

            var bytes = Encoding.ASCII.GetBytes("Studio");
            context.WriteBytes(field.SchemaField.Offset, bytes.AsSpan());

            CollectionAssert.AreEqual(new[] { nameof(field.Text) }, recorder.ChangedProperties);
            Assert.AreEqual(originalValue, field.Text);
        }

        [Test]
        public void DataChangeElsewhere_DoesNotTriggerChange()
        {
            var field = CreateField();
            var context = field.Context;
            var recorder = new NotifyChangeRecorder(field);

            var otherField = context.FieldContainer.FieldsByName["KitSubName"];

            var bytes = Encoding.ASCII.GetBytes("abc");
            context.WriteBytes((ModuleOffset) otherField.Offset, bytes.AsSpan());

            // The data in the field wasn't modified, so we didn't report a change.
            Assert.IsEmpty(recorder.ChangedProperties);
        }*/
    }
}
