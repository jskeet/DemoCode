// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public interface IDataField : INotifyPropertyChanged
    {
        string FormattedText { get; }
        /// <summary>
        /// Attempt to parse the given text, matching it to an underlying
        /// value and setting that in the data model.
        /// </summary>
        bool TrySetFormattedText(string text);
        IField SchemaField { get; }
        void Reset();
    }
}
