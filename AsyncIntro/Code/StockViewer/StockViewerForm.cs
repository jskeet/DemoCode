// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using StockMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class StockViewerForm : Form
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IStockPortfolioService portfolioService;
        private readonly IStockPriceService priceService;

        // Preserved for the sake of the Windows Forms designer
        public StockViewerForm() : this(null, null, null)
        {
        }

        public StockViewerForm(IAuthenticationService authenticationService,
            IStockPortfolioService portfolioService,
            IStockPriceService priceService)
        {
            this.authenticationService = authenticationService;
            this.portfolioService = portfolioService;
            this.priceService = priceService;
            InitializeComponent();
        }

        private async void FetchStocks(object sender, EventArgs e)
        {
            debugLog.Clear();
            portfolioListView.Items.Clear();
            Task task = FetchStocksAsync();
            Log("FetchStocksAsync has returned - status {0}", task.Status);
            await task;
            Log("FetchStocksAsync task completed - status {0}", task.Status);
        }

#if PARALLEL
        private async Task FetchStocksAsync()
        {
            try
            {
                fetchButton.Enabled = false;

                Log("Authenticating");
                Task<Guid?> authTask = authenticationService.AuthenticateUserAsync
                    (userTextBox.Text, passwordTextBox.Text);
                Log("Authentication started");
                Guid? userId = await authTask;

                Log("Authenticated - GUID={0}", userId);
                if (userId == null)
                {
                    Log("Authentication failure - abort.");
                    MessageBox.Show("User unknown, or incorrect password",
                        "Authentication failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Log("Fetching holdings");
                List<StockHolding> holdings =
                    await portfolioService.GetPortfolioAsync(userId.Value);
                Log("Received {0} holdings", holdings.Count);

                Log("Fetching prices");
                decimal totalWorth = await FetchPricesParallelAsync(holdings);
                Log("Fetched all prices");
                AddPortfolioEntry("TOTAL", null, null, totalWorth);
            }
            finally
            {
                fetchButton.Enabled = true;
            }
        }

        private async Task<decimal> FetchPricesParallelAsync(IEnumerable<StockHolding> holdings)
        {
            decimal totalWorth = 0m;

            var tasks = holdings.Select(
                async holding => new { holding,
                    price = await priceService.LookupPriceAsync(holding.Ticker) })
                                .ToList();

            foreach (var task in tasks.InCompletionOrder())
            {
                var result = await task;
                var holding = result.holding;
                var price = result.price;
                Log("Price of {0}: {1}", holding.Ticker, price);

                AddPortfolioEntry(
                    holding.Ticker, holding.Quantity,
                    price, holding.Quantity * price);
                if (price != null)
                {
                    totalWorth += holding.Quantity * price.Value;
                }
            }
            return totalWorth;
        }
#else
        private async Task FetchStocksAsync()
        {
            try
            {
                fetchButton.Enabled = false;

                Log("Authenticating");
                Task<Guid?> authTask = authenticationService.AuthenticateUserAsync
                    (userTextBox.Text, passwordTextBox.Text);
                Log("Authentication started");
                Guid? userId = await authTask;

                Log("Authenticated - GUID={0}", userId);
                if (userId == null)
                {
                    Log("Authentication failure - abort.");
                    MessageBox.Show("User unknown, or incorrect password",
                        "Authentication failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Log("Fetching holdings");
                List<StockHolding> holdings =
                    await portfolioService.GetPortfolioAsync(userId.Value);
                Log("Received {0} holdings", holdings.Count);

                Log("Fetching prices");
                decimal totalWorth = 0m;
                foreach (var holding in holdings)
                {
                    decimal? price = await priceService.LookupPriceAsync(holding.Ticker);
                    Log("Price of {0}: {1}", holding.Ticker, price);

                    AddPortfolioEntry(
                        holding.Ticker, holding.Quantity,
                        price, holding.Quantity * price);
                    if (price != null)
                    {
                        totalWorth += holding.Quantity * price.Value;
                    }
                }
                Log("Fetched all prices");
                AddPortfolioEntry("TOTAL", null, null, totalWorth);
            }
            finally
            {
                fetchButton.Enabled = true;
            }
        }

#endif

        private void AddPortfolioEntry(string ticker, int? quantity,
            decimal? price, decimal? value)
        {
            portfolioListView.Items.Add(new ListViewItem(new[] { ticker, quantity.ToString(),
                price.ToString(), value.ToString() }));
        }

        private void Log(string format, params object[] values)
        {
            string text = string.Format(format, values);
            string line = string.Format("{0:HH:mm:ss.fff}: {1}\r\n", DateTime.Now, text);
            debugLog.AppendText(line);
        }
    }
}
