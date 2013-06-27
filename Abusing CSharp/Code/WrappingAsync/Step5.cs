// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace WrappingAsync
{
    class Step5
    {
        static void Main()
        {
            Console.WriteLine(GetDetailsAsync().Result);
        }

        static async Task<dynamic> GetDetailsAsync()
        {
            return await new
            {
                Weight = Service.GetWeightAsync(),
                Name = Service.GetNameAsync(),
                Birthday = Service.GetBirthdayAsync()
            };
        }
    }
}
