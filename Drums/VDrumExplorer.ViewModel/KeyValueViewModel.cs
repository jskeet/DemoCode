// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// A simple view model for use in tables.
    /// </summary>
    public class KeyValueViewModel : ViewModelBase
    {
        public string Key { get; }
        public string Value { get; }
        public string? KeyToolTip { get; }
        public string? ValueToolTip { get; }

        public KeyValueViewModel(string key, string value) : this(key, value, null, null) { }

        public KeyValueViewModel(string key, string value, string? keyToolTip, string? valueToolTip) =>
            (Key, Value, KeyToolTip, ValueToolTip) = (key, value, keyToolTip, valueToolTip);
    }
}
