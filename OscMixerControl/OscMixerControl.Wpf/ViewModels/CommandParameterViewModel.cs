// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Globalization;

namespace OscMixerControl.Wpf.ViewModels
{
    public class CommandParameterViewModel : ViewModelBase
    {
        private delegate bool TryParseDelegate<T>(string value, out T result);

        private static readonly TryParseDelegate<int> Int32Parser = (string value, out int result) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

        private static readonly TryParseDelegate<long> Int64Parser = (string value, out long result) =>
            long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

        private static readonly TryParseDelegate<float> SingleParser = (string value, out float result) =>
            float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        private static readonly TryParseDelegate<double> DoubleParser = (string value, out double result) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        private static readonly TryParseDelegate<bool> BoolParser = (string value, out bool result) =>
            bool.TryParse(value, out result);

        public string Title { get; }

        public CommandParameterViewModel(string title) => Title = title;

        // Valid types:
        /*  !(obj is int) &&
            !(obj is long) &&
            !(obj is float) &&
            !(obj is double) &&
            !(obj is string) &&
            !(obj is bool) &&
            !(obj is OscNull) &&
            !(obj is OscColor) &&
            !(obj is OscSymbol) &&
            !(obj is OscTimeTag) &&
            !(obj is OscMidiMessage) &&
            !(obj is OscImpulse) &&
            !(obj is byte) &&
            !(obj is byte[]))
            */
        
        private string text;
        public string Text
        {
            get => text;
            set
            {
                ConvertParameterValue(value);
                SetProperty(ref text, value);
            }
        }

        internal object Value => ConvertParameterValue(Text);

        internal static object ConvertParameterValue(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return
                TryParse(value, Int32Parser) ??
                TryParse(value, BoolParser) ??
                TryParseSuffixed('f', value, SingleParser) ??
                TryParseSuffixed('d', value, DoubleParser) ??
                TryParseSuffixed('l', value, Int64Parser) ??
                TryParseSuffixed('L', value, Int64Parser) ??
                TryParseString(value) ??
                TryParseNull(value) ??
                throw new ArgumentException($"Unable to parse value '{value}'");
        }
        
        private static object TryParse<T>(string value, TryParseDelegate<T> method) =>
            method.Invoke(value, out var result) ? result : null;

        private static object TryParseSuffixed<T>(char suffix, string value, TryParseDelegate<T> method) =>
            value[value.Length - 1] == suffix
                ? TryParse(value.Substring(0, value.Length - 1), method) : null;

        private static object TryParseString(string value) =>
            value.Length > 2 && value[0] == '"' && value[value.Length - 1] == '"' ? value.Substring(1, value.Length - 2) : null;

        private static object TryParseNull(string value) =>
            value == "null" ? OscNull.Value : null;
    }
}
