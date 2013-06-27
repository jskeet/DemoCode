// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace WrappingAsync
{
    class Step1
    {
        static void Main()
        {
            Console.WriteLine(GetDetailsAsync().Result);
        }

        static async Task<object> GetDetailsAsync()
        {
            var weightTask = Service.GetWeightAsync();
            var nameTask = Service.GetNameAsync();
            var birthdayTask = Service.GetBirthdayAsync();

            return new 
            { 
                Weight = await weightTask,
                Name = await nameTask, 
                Birthday = await birthdayTask
            };
        }
    }
}
