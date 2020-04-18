// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Fields
{
    /// <summary>
    /// Base class for "live" fields, backed by a <see cref="ModuleData"/> and performing change events.
    /// Life-cycle:
    /// - Construction of all fields
    /// - Fields resolved for all fields
    /// - Load called for all fields
    /// 
    /// Overlay field lists do *not* have fields resolved.
    /// </summary>
    public abstract class DataFieldBase : IDataField
    {
        public IField SchemaField => GetSchemaField();

        private PropertyChangedEventHandler? propertyChanged;

        protected virtual void OnPropertyChangedHasSubscribers()
        {
        }

        protected virtual void OnPropertyChangedHasNoSubscribers()
        {
        }

        // Implement the event subscription manually so ViewModels can subscribe and unsubscribe
        // from events raised by their models.
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (NotifyPropertyChangedHelper.AddHandler(ref propertyChanged, value))
                {
                    OnPropertyChangedHasSubscribers();
                }
            }
            remove
            {
                if (NotifyPropertyChangedHelper.RemoveHandler(ref propertyChanged, value))
                {
                    OnPropertyChangedHasNoSubscribers();
                }
            }
        }

        protected virtual void RaisePropertyChanges() =>
            RaisePropertyChanged(nameof(FormattedText));

        protected void RaisePropertyChanged(string name) =>
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetProperty<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            RaisePropertyChanges();
            return true;
        }

        public abstract string FormattedText { get; }
        protected abstract IField GetSchemaField();
        public abstract void Reset();

        internal virtual void ResolveFields(ModuleData data, FieldContainer container)
        {
        }

        internal abstract void Load(DataSegment segment);
        internal abstract void Save(DataSegment segment);

        internal static IDataField CreateDataField(IField field, ModuleSchema schema)
        {
            return field switch
            {
                StringField f => new StringDataField(f),
                BooleanField f => new BooleanDataField(f),
                NumericField f => new NumericDataField(f),
                EnumField f => new EnumDataField(f),
                InstrumentField f => new InstrumentDataField(f, schema),
                OverlayField f => new OverlayDataField(f, schema),
                _ => throw new ArgumentException($"Can't handle {field} yet")
            };
        }
    }

    public abstract class DataFieldBase<TField> : DataFieldBase where TField : IField
    {
        /// <summary>
        /// The field in the schema.
        /// </summary>
        public new TField SchemaField { get; }

        protected override IField GetSchemaField() => SchemaField;

        // Convenience properties to access schema field properties
        protected ModuleOffset Offset => SchemaField.Offset;
        protected int Size => SchemaField.Size;

        protected DataFieldBase(TField schemaField)
        {
            SchemaField = schemaField;
        }
    }
}
