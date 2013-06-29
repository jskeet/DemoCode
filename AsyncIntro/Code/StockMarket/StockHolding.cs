// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
namespace StockMarket
{
    /// <summary>
    /// A simple ticker/quantity pair representing a number of shares in a particular company.
    /// </summary>
    public class StockHolding
    {
        public string Ticker { get; private set; }
        public int Quantity { get; private set; }

        public StockHolding(string ticker, int quantity)
        {
            Ticker = ticker;
            Quantity = quantity;
        }
    }
}
