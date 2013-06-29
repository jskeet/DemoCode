// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using StockMarket;
using System.Threading.Tasks;

namespace AsyncHttpService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var handler = new HttpJsonRpcHandler(8888, "async");
            handler.AddTarget("auth", new SimpleAuthenticationService());
            handler.AddTarget("portfolio", new SimpleStockPortfolioService());
            handler.AddTarget("price", new SimpleStockPriceService());

            Task task = handler.Start();
            task.Wait();
        }
    }
}
