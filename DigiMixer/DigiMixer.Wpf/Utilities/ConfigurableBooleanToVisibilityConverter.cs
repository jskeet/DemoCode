using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigiMixer.Wpf.Utilities;

public class ConfigurableBooleanToVisibilityConverter : IValueConverter
{
    public Visibility TrueVisibility { get; set; }
    public Visibility FalseVisibility { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? TrueVisibility : FalseVisibility;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
