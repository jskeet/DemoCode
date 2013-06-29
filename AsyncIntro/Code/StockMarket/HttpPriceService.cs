// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Threading.Tasks;

namespace StockMarket
{
    public class HttpPriceService : IStockPriceService
    {
        private readonly string url;

        public HttpPriceService(string url)
        {
            this.url = url;
        }

        public Task<decimal?> LookupPriceAsync(string ticker)
        {
            return HttpJsonRpcServiceHelper.ExecuteRequestAsync<decimal?>(url, new { ticker });
        }
    }
}
