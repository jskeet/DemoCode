// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace StockMarket
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates the given user/password, returning the user's ID
        /// as a GUID on success, or null on failure.
        /// </summary>
        Task<Guid?> AuthenticateUserAsync(string user, string password);
    }
}
