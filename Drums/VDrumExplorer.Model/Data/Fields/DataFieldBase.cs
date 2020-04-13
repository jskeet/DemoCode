// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.ComponentModel;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Fields
{
    /// <summary>
    /// Base class for "live" fields, backed by a <see cref="ModuleData"/> and performing change events.
    /// </summary>
    public abstract class DataFieldBase : IDataField
    {
        public IField SchemaField => GetSchemaField();
        public FieldContainerData Context { get; }

        protected DataFieldBase(FieldContainerData context) =>
            Context = context;

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

        protected void RaisePropertyChange(string name) => propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public abstract string FormattedText { get; }
        protected abstract IField GetSchemaField();
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

        private Func<DataChangedEventArgs, bool> dataChangeMatcher = change => false;

        protected DataFieldBase(FieldContainerData context, TField schemaField, bool subscribeToFieldChanges) : base(context)
        {
            SchemaField = schemaField;
            if (subscribeToFieldChanges)
            {
                AddFieldMatcher(Context.FieldContainer, SchemaField);
            }
        }

        protected DataFieldBase(FieldContainerData context, TField schemaField) : this(context, schemaField, true)
        {
        }

        protected void AddFieldMatcher(FieldContainer container, IField field)
        {
            var existing = dataChangeMatcher;
            dataChangeMatcher = change => existing(change) || change.OverlapsField(container, field);
        }            

        protected override void OnPropertyChangedHasSubscribers()
        {
            Context.DataChanged += ContainerDataChanged;
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            Context.DataChanged -= ContainerDataChanged;
        }

        protected virtual void OnDataChanged()
        {
            RaisePropertyChange(nameof(FormattedText));
        }

        protected void ContainerDataChanged(object sender, DataChangedEventArgs e)
        {
            if (dataChangeMatcher(e))
            {
                OnDataChanged();
            }
        }
    }
}
