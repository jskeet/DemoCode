using System;
using System.Globalization;
using System.Windows.Data;

namespace OscMixerControl.Wpf
{
    /// <summary>
    /// Converter from dB levels (non-positive, division by 256 to get to dB)
    /// to a value that spaces the following ranges equally in the range 0-1:
    /// - -5dB to 0dB
    /// - -10dB to -5dB
    /// - -20dB to -10dB
    /// - -30dB to -20dB
    /// - -40dB to -30dB
    /// - -50dB to -40dB
    /// - -75dB to -50dB
    /// Anything lower than -75dB ends up with a value of 0.
    /// </summary>
    public class OutputLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double db)
            {
                throw new ArgumentException();
            }
            // Range of 0-700, which we then scale to 0-1.
            double ranged =  db switch
            {      
                >= -5d => (db + 5d) * (100d / 5d) + 600,
                >= -10d => (db + 10d) * (100d / 5d) + 500,
                >= -20d => (db + 20d) * (100d / 10d) + 400,
                >= -30d => (db + 30d) * (100d / 10d) + 300,
                >= -40d => (db + 40d) * (100d / 10d) + 200,
                >= -50d => (db + 50d) * (100d / 10d) + 100,
                >= -75d => (db + 75d) * (100d / 25d) + 0,
                _ => 0f
            };
            return ranged / 700d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
