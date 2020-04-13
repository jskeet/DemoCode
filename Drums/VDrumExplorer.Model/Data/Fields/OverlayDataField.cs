// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Fields
{
    public class OverlayDataField : DataFieldBase<OverlayField>
    {
        private IDataField switchField;

        private readonly IReadOnlyList<Lazy<FieldList>> fieldLists;

        // We don't need to subscribe to field changes for this field that covers all the values; the individual fields will take
        // care of that. We do need to subscribe to changes for the switch field though.
        public OverlayDataField(FieldContainerData context, OverlayField field) : base(context, field, subscribeToFieldChanges: false)
        {
            var (switchContainer, switchSchemaField) = context.FieldContainer.ResolveField(SchemaField.SwitchPath);
            switchField = context.ModuleData.CreateDataField(switchContainer, switchSchemaField);
            fieldLists = field.FieldLists.ToReadOnlyList(fl => Lazy.Create(() => new FieldList(context, fl)));
            AddFieldMatcher(switchContainer, switchSchemaField);
        }

        protected override void OnPropertyChangedHasSubscribers()
        {
            base.OnPropertyChangedHasSubscribers();
            if (switchField.Context != Context)
            {
                switchField.Context.DataChanged += ContainerDataChanged;
            }
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            base.OnPropertyChangedHasNoSubscribers();
            if (switchField.Context != Context)
            {
                switchField.Context.DataChanged -= ContainerDataChanged;
            }
        }

        public FieldList GetFieldList()
        {
            var index = switchField switch
            {
                InstrumentDataField instrument => instrument.Instrument.Group?.Index ?? Context.FieldContainer.Schema.InstrumentGroups.Count,
                NumericDataField numeric => numeric.RawValue,
                EnumDataField enumField => enumField.RawValue,
                _ => throw new InvalidOperationException($"Invalid field type for overlay switch: {switchField.GetType()}")
            };
            return fieldLists[index].Value;
        }

        /// <summary>
        /// The "data" version of <see cref="OverlayField.FieldList"/>
        /// </summary>
        public class FieldList
        {
            public string Description { get; }
            public IReadOnlyList<IDataField> Fields { get; }

            public FieldList(FieldContainerData context, OverlayField.FieldList schemaFieldList)
            {
                Description = schemaFieldList.Description;
                Fields = schemaFieldList.Fields.ToReadOnlyList(field => context.ModuleData.CreateDataField(context.FieldContainer, field));
            }
        }

        public override string FormattedText => throw new InvalidOperationException($"{nameof(OverlayDataField)} cannot be formatted");
    }
}
