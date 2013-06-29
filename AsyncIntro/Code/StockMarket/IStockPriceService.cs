// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Threading.Tasks;

namespace StockMarket
{
    public interface IStockPriceService
    {
        /// <summary>
        /// Looks up the price of shares for a given company stock ticker, returning
        /// the known price or null if the ticker is unknown.
        /// </summary>
        Task<decimal?> LookupPriceAsync(string ticker);
    }
}
