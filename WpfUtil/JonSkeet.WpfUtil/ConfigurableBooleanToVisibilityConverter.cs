// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JonSkeet.WpfUtil;

public class ConfigurableBooleanToVisibilityConverter : IValueConverter
{
    public Visibility TrueVisibility { get; set; }
    public Visibility FalseVisibility { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? TrueVisibility : FalseVisibility;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
