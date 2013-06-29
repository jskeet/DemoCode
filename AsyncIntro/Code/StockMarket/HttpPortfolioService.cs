// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockMarket
{
    public class HttpPortfolioService : IStockPortfolioService
    {
        private readonly string url;

        public HttpPortfolioService(string url)
        {
            this.url = url;
        }

        public Task<List<StockHolding>> GetPortfolioAsync(Guid userId)
        {
            return HttpJsonRpcServiceHelper.ExecuteRequestAsync<List<StockHolding>>(url, new { userId });
        }
    }
}
