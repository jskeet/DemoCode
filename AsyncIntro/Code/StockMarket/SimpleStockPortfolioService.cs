// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockMarket
{
    public sealed class SimpleStockPortfolioService : IStockPortfolioService
    {
        private readonly SimpleDataContext data;

        public SimpleStockPortfolioService()
        {
            data = SimpleDataContext.DemoData;
        }

        public async Task<List<StockHolding>> GetPortfolioAsync(Guid userId)
        {
            await Task.Delay(RandomProvider.GetThreadRandom().Next(2000));

            var holdingsForUser = data.Holdings
                                      .Where(h => h.UserId == userId)
                                      .Select(h => new StockHolding(h.Ticker, h.Quantity))
                                      .ToList();
            return holdingsForUser;
            
        }
    }
}
