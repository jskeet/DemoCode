// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace StockMarket
{
    internal sealed class User
    {
        internal string UserName { get; set; }
        internal Guid Id { get; set; }
        // No, we would never *really* do this...
        internal string Password { get; set; }
    }
}
