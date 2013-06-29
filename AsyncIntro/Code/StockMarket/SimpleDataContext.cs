// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;

namespace StockMarket
{
    internal class SimpleDataContext
    {
        internal IEnumerable<User> Users { get; private set; }
        internal IEnumerable<UserStockHolding> Holdings { get; private set; }
        internal IEnumerable<StockPrice> Prices { get; private set; }

        internal static SimpleDataContext DemoData { get; private set; }

        static SimpleDataContext()
        {
            DemoData = new SimpleDataContext();
            User jon = new User { UserName = "jon", Password = "c#5", Id = Guid.NewGuid() };
            User rob = new User { UserName = "rob", Password = "ruby", Id = Guid.NewGuid() };

            DemoData.Users = new List<User> { jon, rob };
            DemoData.Holdings = new List<UserStockHolding>
            {
                new UserStockHolding { UserId = jon.Id, Ticker = "acme", Quantity = 20 },
                new UserStockHolding { UserId = jon.Id, Ticker = "abcd", Quantity = 5 },
                new UserStockHolding { UserId = jon.Id, Ticker = "efgh", Quantity = 10 },
                new UserStockHolding { UserId = jon.Id, Ticker = "ijkl", Quantity = 1 },
                new UserStockHolding { UserId = jon.Id, Ticker = "mnop", Quantity = 15 },
                new UserStockHolding { UserId = rob.Id, Ticker = "acme", Quantity = 5 },
                new UserStockHolding { UserId = rob.Id, Ticker = "xyz", Quantity = 10 },
                new UserStockHolding { UserId = rob.Id, Ticker = "tkpb", Quantity = 3 }
            };

            DemoData.Prices = new List<StockPrice>
            {
                new StockPrice { Ticker = "acme", Price = 10.50m },
                new StockPrice { Ticker = "abcd", Price = 13.92m },
                new StockPrice { Ticker = "xyz", Price = 5.67m },
                new StockPrice { Ticker = "tkpb", Price = 58.95m },
                new StockPrice { Ticker = "ijkl", Price = 10m },
                new StockPrice { Ticker = "mnop", Price = 20m }
            };
        }
    }
}
