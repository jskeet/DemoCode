// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace WrappingAsync
{
    class Step3
    {
        static void Main()
        {
            Console.WriteLine(GetDetailsAsync().Result);
        }

        static async Task<Tuple<double, string, DateTime>> GetDetailsAsync()
        {
            var tuple = Tuple.Create(
                Service.GetWeightAsync(),
                Service.GetNameAsync(),
                Service.GetBirthdayAsync());
            return await tuple;
        }
    }
}
