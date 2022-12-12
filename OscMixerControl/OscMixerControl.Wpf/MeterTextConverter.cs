// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using System.Windows.Data;

namespace OscMixerControl.Wpf
{
    /// <summary>
    /// Converter from meter values (16-bit integers) to text (dB).
    /// </summary>
    public class MeterTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double db)
            {
                throw new ArgumentException();
            }
            // TODO: Caching?
            return db + " dB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
