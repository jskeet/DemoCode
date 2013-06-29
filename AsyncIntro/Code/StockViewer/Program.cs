// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using StockMarket;
using System;
using System.Windows.Forms;

namespace StockViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            IAuthenticationService authService = new HttpAuthenticationService(
                "http://localhost:8888/async/auth/AuthenticateUserAsync");
            IStockPortfolioService portfolioService = new HttpPortfolioService(
                "http://localhost:8888/async/portfolio/GetPortfolioAsync");
            IStockPriceService priceService = new HttpPriceService(
                "http://localhost:8888/async/price/LookupPriceAsync");

            //IAuthenticationService authService = new SimpleAuthenticationService();
            //IStockPortfolioService portfolioService = new SimpleStockPortfolioService();
            //IStockPriceService priceService = new SimpleStockPriceService();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StockViewerForm(authService, portfolioService, priceService));
        }
    }
}
