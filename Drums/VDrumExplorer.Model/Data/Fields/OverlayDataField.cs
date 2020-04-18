// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Fields
{
    public class OverlayDataField : DataFieldBase<OverlayField>
    {
        private readonly ModuleSchema schema;
        private IDataField? switchField;
        private string switchIndex;

        private readonly IReadOnlyDictionary<string, Lazy<FieldList>> fieldLists;

        internal OverlayDataField(OverlayField field, ModuleSchema schema) : base(field)
        {
            this.schema = schema;
            fieldLists = SchemaField.FieldLists
                .ToDictionary(
                    pair => pair.Key,
                    pair => Lazy.Create(() => new FieldList(pair.Value, schema)))
                .AsReadOnly();
            switchIndex = "";
        }

        internal override void ResolveFields(ModuleData data, FieldContainer container)
        {
            var resolved = container.ResolveField(SchemaField.SwitchPath);
            switchField = data.GetDataField(resolved.container, resolved.field);
            RefreshInstrumentFields();
        }

        protected override void RaisePropertyChanges()
        {
            RaisePropertyChanged(nameof(CurrentFieldList));
        }

        public override void Reset()
        {
            foreach (var field in CurrentFieldList.Fields)
            {
                field.Reset();
            }
        }

        private string GetSwitchIndex() => switchField switch
        {
            InstrumentDataField instrument => instrument.Instrument.Group.VEditCategory,
            EnumDataField enumField => enumField.Value,
            _ => throw new InvalidOperationException($"Invalid field type for overlay switch: {switchField!.GetType()}")
        };

        void SwitchFieldChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO: If we change for "Splash" to "China" we don't actually need to reset things.
            if (SetProperty(ref switchIndex, GetSwitchIndex()))
            {
                Reset();
            }
            RefreshInstrumentFields();
        }

        void RefreshInstrumentFields()
        {
            if (switchField is InstrumentDataField instrumentSwitch)
            {
                var instrument = instrumentSwitch.Instrument;
                if (instrument.DefaultFieldValues is object)
                {
                    foreach (var field in CurrentFieldList.Fields.OfType<NumericDataFieldBase>())
                    {
                        if (instrument.DefaultFieldValues.TryGetValue(field.SchemaField.Name, out int? rawValue))
                        {
                            field.RawValue = rawValue ?? field.SchemaField.Default;
                        }
                    }
                }
            }
        }

        protected override void OnPropertyChangedHasSubscribers()
        {
            base.OnPropertyChangedHasSubscribers();
            switchField!.PropertyChanged += SwitchFieldChanged;
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            base.OnPropertyChangedHasNoSubscribers();
            switchField!.PropertyChanged -= SwitchFieldChanged;
        }

        internal override void Load(DataSegment segment)
        {
            // FIXME: Can we assume that the switch field has already been loaded? Feels brittle.
            // Probably okay if we validate it in tests.
            switchIndex = GetSwitchIndex();
            foreach (DataFieldBase field in CurrentFieldList.Fields)
            {
                field.Load(segment);
            }
        }

        internal override void Save(DataSegment segment)
        {
            // FIXME: Can we assume that the switch field has already been loaded? Feels brittle.
            foreach (DataFieldBase field in CurrentFieldList.Fields)
            {
                field.Save(segment);
            }
        }

        public FieldList CurrentFieldList => fieldLists[switchIndex].Value;

        /// <summary>
        /// The "data" version of <see cref="OverlayField.FieldList"/>
        /// </summary>
        public class FieldList
        {
            public string Description { get; }
            public IReadOnlyList<IDataField> Fields { get; }

            public FieldList(OverlayField.FieldList schemaFieldList, ModuleSchema schema)
            {
                Description = schemaFieldList.Description;
                Fields = schemaFieldList.Fields.ToReadOnlyList(field => DataFieldBase.CreateDataField(field, schema));
            }
        }

        public override string FormattedText => throw new InvalidOperationException($"{nameof(OverlayDataField)} cannot be formatted");
    }
}
