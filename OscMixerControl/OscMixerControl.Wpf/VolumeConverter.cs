// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using System.Windows.Data;

namespace OscMixerControl.Wpf
{
    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int level)
            {
                throw new ArgumentException();
            }
            float floatLevel = level / 1024f;
            double d = floatLevel switch
            {
                float f when f >= 0.5f => f * 40 -30,
                float f when f >= 0.25f => f * 80 - 50,
                float f when f >= 0.0625f => f * 160 - 70,
                float f when f == 0f => double.NegativeInfinity,
                float f => f * 480 - 90
            };
            string formattedDb = double.IsNegativeInfinity(d)
                ? "-\u221e"
                : d.ToString("0.0", CultureInfo.InvariantCulture);
            return formattedDb + "dB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
