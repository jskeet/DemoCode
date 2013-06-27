// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace WrappingAsync
{
    class Service
    {
        public static async Task<double> GetWeightAsync()
        {
            await Task.Delay(100);
            return 50d;
        }

        public static async Task<DateTime> GetBirthdayAsync()
        {
            await Task.Delay(50);
            return new DateTime(1976, 6, 19);
        }

        public static async Task<string> GetNameAsync()
        {
            await Task.Delay(200);
            return "fred";
        }
    }
}
