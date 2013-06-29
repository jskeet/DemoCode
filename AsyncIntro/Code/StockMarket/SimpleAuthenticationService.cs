// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockMarket
{
    public sealed class SimpleAuthenticationService : IAuthenticationService
    {
        private readonly SimpleDataContext data;

        public SimpleAuthenticationService()
        {
            data = SimpleDataContext.DemoData;
        }

        public async Task<Guid?> AuthenticateUserAsync(string user, string password)
        {
            await Task.Delay(5000);

            Guid? id = data.Users
                           .Where(u => u.UserName == user && u.Password == password)
                           .Select(u => (Guid?) u.Id)
                           .SingleOrDefault();
            return id;
        }
    }
}
