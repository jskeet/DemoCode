// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Linq;
using System.Threading.Tasks;

namespace StockMarket
{
    public sealed class SimpleStockPriceService : IStockPriceService
    {
        private readonly SimpleDataContext data;

        public SimpleStockPriceService()
        {
            data = SimpleDataContext.DemoData;
        }

        public async Task<decimal?> LookupPriceAsync(string ticker)
        {
            if (ticker == "acme")
            {
                await Task.Delay(3000);
            }
            await Task.Delay(1000 + RandomProvider.GetThreadRandom().Next(1000));

            decimal? price = data.Prices
                                 .Where(p => p.Ticker == ticker)
                                 .Select(p => (decimal?)p.Price)
                                 .SingleOrDefault();
            return price;
        }
    }
}
