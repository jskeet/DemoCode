// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JonSkeet.WpfLogging;

public class LogLevelBrushConverter : IValueConverter
{
    public Brush DebugBrush { get; set; }
    public Brush InfoBrush { get; set; }
    public Brush ErrorBrush { get; set; }
    public Brush WarnBrush { get; set; }
    public Brush CriticalBrush { get; set; }
    public Brush TraceBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is LogLevel level ? ConvertLogLevel(level) : Brushes.Black;

    private Brush ConvertLogLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => TraceBrush ?? Brushes.Black,
        LogLevel.Information => InfoBrush ?? Brushes.Black,
        LogLevel.Debug => DebugBrush ?? Brushes.Black,
        LogLevel.Error => ErrorBrush ?? Brushes.Black,
        LogLevel.Critical => CriticalBrush ?? Brushes.Black,
        LogLevel.Warning => WarnBrush ?? Brushes.Black,
        _ => Brushes.Black
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
