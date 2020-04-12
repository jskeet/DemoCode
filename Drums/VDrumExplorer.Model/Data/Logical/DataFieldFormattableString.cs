// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Logical
{
    public class DataFieldFormattableString : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<IDataField> fields;
        private readonly string formatString;

        private PropertyChangedEventHandler? propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (NotifyPropertyChangedHelper.AddHandler(ref propertyChanged, value))
                {
                    SubscribeToFields();
                }
            }
            remove
            {
                if (NotifyPropertyChangedHelper.RemoveHandler(ref propertyChanged, value))
                {
                    UnsubscribeFromFields();
                }
            }
        }

        public string Text =>
            fields.Count == 0
            ? formatString
            : string.Format(formatString, fields.Select(f => f.FormattedText).ToArray());

        public DataFieldFormattableString(ModuleData data, FieldFormattableString formattable)
        {
            fields = formattable.Fields?.ToReadOnlyList(pair => data.CreateDataField(pair.container, pair.field))
                ?? new DataFieldBase[0];
            formatString = formattable.FormatString;
        }
        
        private void FieldHasChanged(object sender, PropertyChangedEventArgs e) =>
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));

        private void SubscribeToFields()
        {
            foreach (var field in fields)
            {
                field.PropertyChanged += FieldHasChanged;
            }
        }

        private void UnsubscribeFromFields()
        {
            foreach (var field in fields)
            {
                field.PropertyChanged -= FieldHasChanged;
            }
        }
    }
}
