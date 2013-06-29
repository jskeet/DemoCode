// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
#define PARALLEL

using System.Linq;
using StockMarket;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Testing.NUnit
{
    public class StockBroker
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IStockPortfolioService portfolioService;
        private readonly IStockPriceService priceService;

        public StockBroker(IAuthenticationService authenticationService,
                           IStockPortfolioService portfolioService,
                           IStockPriceService priceService)
        {
            this.authenticationService = authenticationService;
            this.portfolioService = portfolioService;
            this.priceService = priceService;
        }

        public Task<decimal> CalculateWorthAsync(string user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            return CalculateWorthAsyncImpl(user, password);
        }

        private async Task<decimal> CalculateWorthAsyncImpl(string user, string password)
        {
            var userId = await authenticationService.AuthenticateUserAsync(user, password);
            if (userId == null)
            {
                throw new AuthenticationException("Bad username or password");
            }

            var portfolio = await portfolioService.GetPortfolioAsync(userId.Value);

            decimal total = 0m;

#if !PARALLEL
#region Serial implementation
            foreach (var item in portfolio)
            {
                var price = await priceService.LookupPriceAsync(item.Ticker);
                if (price != null)
                {
                    total += price.Value * item.Quantity;
                }
            }
#endregion
#else
#region Parallel implementation
            var tasks = portfolio.Select(async holding => new
                {
                    holding,
                    price = await priceService.LookupPriceAsync(holding.Ticker)
                }).ToList();

            foreach (var task in tasks.InCompletionOrder())
            {
                var result = await task;
                var holding = result.holding;
                var price = result.price;
                if (price != null)
                {
                    total += holding.Quantity * price.Value;
                }
            }
#endregion
#endif
            return total;
        }
    }
}
