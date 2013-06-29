// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockMarket
{
    public interface IStockPortfolioService
    {
        /// <summary>
        /// Fetches the stock portfolio for the specified user ID.
        /// </summary>
        Task<List<StockHolding>> GetPortfolioAsync(Guid userId);
    }
}
