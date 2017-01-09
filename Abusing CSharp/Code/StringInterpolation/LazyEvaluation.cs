// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using static StringInterpolation.Capture;

namespace StringInterpolation
{
    class LazyEvaluation
    {
        static void Main()
        {
            string value = "Before";
            FormattableString formattable = $"Current value: {_(() => value)}";
            Console.WriteLine(formattable);

            value = "After";
            Console.WriteLine(formattable);
        }
    }

    class Capture : IFormattable
    {
        private readonly Func<object> provider;

        public static Capture _(Func<object> provider)
        {
            return new Capture(provider);
        }

        public Capture(Func<object> provider)
        {
            this.provider = provider;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            object value = provider();
            if (value == null)
            {
                return "";
            }
            var formattable = value as IFormattable;
            return formattable == null ? value.ToString() : formattable.ToString(format, formatProvider);
        }
    }
}
