// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using StockMarket;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Testing.MsTest
{
    [TestClass]
    public class StockBrokerTest
    {
        private StockBroker broker;
        private IAuthenticationService authService;
        private IStockPortfolioService portfolioService;
        private IStockPriceService priceService;

        [TestInitialize]
        public void SetUp()
        {
            authService = MockRepository.GenerateStub<IAuthenticationService>();
            portfolioService = MockRepository.GenerateStub<IStockPortfolioService>();
            priceService = MockRepository.GenerateStub<IStockPriceService>();
            broker = new StockBroker(authService, portfolioService, priceService);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateNetWorthAsync_NullUser_ThrowsImmediately()
        {
            broker.CalculateWorthAsync(null, "pass");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateNetWorthAsync_NullPassword_ThrowsImmediately()
        {
            broker.CalculateWorthAsync("jon", null);
        }

        [TestMethod]
        public async Task CalculateNetWorthAsync_AuthenticationFailure_ThrowsDelayed()
        {
            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(Delayed<Guid?>(100, null));

            await AssertThrows<AuthenticationException>(broker.CalculateWorthAsync("jon", "pass"));
        }

        [TestMethod]
        public async Task CalculateNetWorth_EmptyPortfolio()
        {
            var guid = Guid.NewGuid();
            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(Delayed<Guid?>(100, guid));
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(Delayed(200, new List<StockHolding>()));

            Assert.AreEqual(0m, await broker.CalculateWorthAsync("jon", "pass"));
        }

        [TestMethod]
        public async Task CalculateNetWorth_SingleItem()
        {
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10)
            };

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(Delayed<Guid?>(100, guid));
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(Delayed(200, portfolio));
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(Delayed<decimal?>(300, 34.25m));

            Assert.AreEqual(10 * 34.25m, await broker.CalculateWorthAsync("jon", "pass"));
        }

        [TestMethod]
        public async Task CalculateNetWorth_MultipleItems()
        {
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10),
                new StockHolding("BAR", 5)
            };

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(Delayed<Guid?>(0, guid));
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(Delayed(100, portfolio));
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(Delayed<decimal?>(200, 20m));
            priceService.Stub(x => x.LookupPriceAsync("BAR")).Return(Delayed<decimal?>(100, 10m));

            Assert.AreEqual(10 * 20m + 5 * 10m, await broker.CalculateWorthAsync("jon", "pass"));
        }

        [TestMethod]
        public async Task CalculateNetWorth_MultipleItems_IncludingNullValue()
        {
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10),
                new StockHolding("BAR", 5)
            };

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(Delayed<Guid?>(0, guid));
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(Delayed(100, portfolio));
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(Delayed<decimal?>(200, 20m));
            priceService.Stub(x => x.LookupPriceAsync("BAR")).Return(Delayed<decimal?>(100, null));

            Assert.AreEqual(10 * 20m, await broker.CalculateWorthAsync("jon", "pass"));
        }

        private async Task<T> Delayed<T>(int milliseconds, T result)
        {
            await Task.Delay(milliseconds);
            return result;
        }

        private async Task AssertThrows<TException>(Task task) where TException : Exception
        {
            try
            {
                await task;
                Assert.Fail("Expected exception of type " + typeof(TException));
            }
            catch (TException)
            {                
                // Expected
            }
        }
    }
}
