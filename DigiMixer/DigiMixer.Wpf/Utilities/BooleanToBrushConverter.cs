using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DigiMixer.Wpf.Utilities;

public class BooleanToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; }
    public Brush FalseBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? TrueBrush : FalseBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
