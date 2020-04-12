// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VDrumExplorer.Model.Test
{
    public class NotifyChangeRecorder
    {
        private readonly List<string> changedProperties;
        public IReadOnlyList<string> ChangedProperties { get; }

        public NotifyChangeRecorder(INotifyPropertyChanged model)
        {
            changedProperties = new List<string>();
            ChangedProperties = changedProperties.AsReadOnly();
            model.PropertyChanged += RecordChange;
        }

        private void RecordChange(object sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName);
    }
}
