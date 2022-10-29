using DigiMixer.Core;
using System.Globalization;
using System.Windows.Data;

namespace DigiMixer.Wpf
{
    /// <summary>
    /// Converter from <see cref="Core.MeterLevel"/> levels (non-positive dB)
    /// to a value that spaces the following ranges equally 
    /// - -5dB to 0dB     => 500 to 600
    /// - -10dB to -5dB   => 400 to 500
    /// - -20dB to -10dB  => 300 to 400
    /// - -30dB to -20dB  => 200 to 300
    /// - -50dB to -30dB  => 100 to 200
    /// - -75dB to -50dB  => 0 to 100
    /// Anything lower than -75dB ends up with a value of 0.
    /// </summary>
    public class OutputLevelLogConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MeterLevel level)
            {
                throw new ArgumentException();
            }
            var db = level.Value;
            return db switch
            {      
                >= -5f => (db + 5f) * (100f / 5f) + 500,
                >= -10f => (db + 10f) * (100f / 5f) + 400,
                >= -20f => (db + 20f) * (100f / 10f) + 300,
                >= -30f => (db + 30f) * (100f / 10f) + 200,
                >= -50f => (db + 50f) * (100f / 20f) + 100,
                >= -75f => (db + 75f) * (100f / 25) + 0,
                _ => 0f
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
