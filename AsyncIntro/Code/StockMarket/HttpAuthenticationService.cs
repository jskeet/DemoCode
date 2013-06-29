// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Threading.Tasks;

namespace StockMarket
{
    public class HttpAuthenticationService : IAuthenticationService
    {
        private readonly string url;

        public HttpAuthenticationService(string url)
        {
            this.url = url;
        }

        public Task<Guid?> AuthenticateUserAsync(string user, string password)
        {
            return HttpJsonRpcServiceHelper.ExecuteRequestAsync<Guid?>(url, new { user, password });
        }
    }
}
