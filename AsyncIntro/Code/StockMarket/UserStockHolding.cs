// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace StockMarket
{
    internal sealed class UserStockHolding
    {
        internal Guid UserId { get; set; }
        internal string Ticker { get; set; }
        internal int Quantity { get; set; }
    }
}
