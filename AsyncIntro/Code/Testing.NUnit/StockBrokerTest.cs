// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using NUnit.Framework;
using Rhino.Mocks;
using StockMarket;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Testing.NUnit
{
    [TestFixture]
    public class StockBrokerTest
    {
        private StockBroker broker;
        private IAuthenticationService authService;
        private IStockPortfolioService portfolioService;
        private IStockPriceService priceService;

        [SetUp]
        public void SetUp()
        {
            authService = MockRepository.GenerateStub<IAuthenticationService>();
            portfolioService = MockRepository.GenerateStub<IStockPortfolioService>();
            priceService = MockRepository.GenerateStub<IStockPriceService>();
            broker = new StockBroker(authService, portfolioService, priceService);
        }

        [Test]
        public void CalculateNetWorthAsync_NullUser_ThrowsImmediately()
        {
            Assert.Throws<ArgumentNullException>(() => broker.CalculateWorthAsync(null, "pass"));
        }

        [Test]
        public void CalculateNetWorthAsync_NullPassword_ThrowsImmediately()
        {
            Assert.Throws<ArgumentNullException>(() => broker.CalculateWorthAsync("jon", null));
        }

        [Test]
        public void CalculateNetWorthAsync_AuthenticationFailure_ThrowsDelayed()
        {
            var tardis = new TimeMachine();
            var failedLogin = tardis.ScheduleSuccess<Guid?>(1, null);
            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(failedLogin);

            tardis.ExecuteInContext(advancer =>
            {
                var worth = broker.CalculateWorthAsync("jon", "pass");
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                AssertFaulted<AuthenticationException>(worth);
            });
        }

        [Test]
        public void CalculateNetWorth_EmptyPortfolio()
        {
            var tardis = new TimeMachine();
            var guid = Guid.NewGuid();
            var loginResult = tardis.ScheduleSuccess<Guid?>(1, guid);
            var portfolioResult = tardis.ScheduleSuccess(2, new List<StockHolding>());
            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(loginResult);
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(portfolioResult);

            tardis.ExecuteInContext(advancer =>
            {
                var worth = broker.CalculateWorthAsync("jon", "pass");
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                AssertCompletion(worth, 0);
            });
        }

        [Test]
        public void CalculateNetWorth_SingleItem()
        {
            var tardis = new TimeMachine();
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10)
            };
            var loginResult = tardis.ScheduleSuccess<Guid?>(1, guid);
            var portfolioResult = tardis.ScheduleSuccess(2, portfolio);
            var priceResult = tardis.ScheduleSuccess<decimal?>(3, 34.25m);

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(loginResult);
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(portfolioResult);
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(priceResult);

            tardis.ExecuteInContext(advancer =>
            {
                var worth = broker.CalculateWorthAsync("jon", "pass");
                // Not complete until auth has completed...
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                // Not complete until portfolio fetch has completed...
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                // Not complete until price fetch has completed...
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                AssertCompletion(worth, 10 * 34.25m);
            });
        }

        [Test]
        public void CalculateNetWorth_MultipleItems()
        {
            var tardis = new TimeMachine();
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10),
                new StockHolding("BAR", 5)
            };
            var loginResult = tardis.ScheduleSuccess<Guid?>(1, guid);
            var portfolioResult = tardis.ScheduleSuccess(2, portfolio);
            var fooResult = tardis.ScheduleSuccess<decimal?>(3, 20m);
            var barResult = tardis.ScheduleSuccess<decimal?>(4, 10m);

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(loginResult);
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(portfolioResult);
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(fooResult);
            priceService.Stub(x => x.LookupPriceAsync("BAR")).Return(barResult);

            tardis.ExecuteInContext(advancer =>
            {
                var worth = broker.CalculateWorthAsync("jon", "pass");
                advancer.AdvanceBy(3);
                // Still waiting, after first price has been fetched...
                Assert.IsFalse(worth.IsCompleted);
                advancer.Advance();
                AssertCompletion(worth, 10 * 20m + 5 * 10m);
            });
        }

        [Test]
        public void CalculateNetWorth_MultipleItems_IncludingNullValue()
        {
            var tardis = new TimeMachine();
            var guid = Guid.NewGuid();
            var portfolio = new List<StockHolding>
            {
                new StockHolding("FOO", 10),
                new StockHolding("BAR", 5)
            };
            var loginResult = tardis.ScheduleSuccess<Guid?>(1, guid);
            var portfolioResult = tardis.ScheduleSuccess(2, portfolio);
            var fooResult = tardis.ScheduleSuccess<decimal?>(3, 20m);
            var barResult = tardis.ScheduleSuccess<decimal?>(4, null);

            authService.Stub(x => x.AuthenticateUserAsync("jon", "pass")).Return(loginResult);
            portfolioService.Stub(x => x.GetPortfolioAsync(guid)).Return(portfolioResult);
            priceService.Stub(x => x.LookupPriceAsync("FOO")).Return(fooResult);
            priceService.Stub(x => x.LookupPriceAsync("BAR")).Return(barResult);

            tardis.ExecuteInContext(advancer =>
            {
                var worth = broker.CalculateWorthAsync("jon", "pass");
                advancer.AdvanceBy(4);
                AssertCompletion(worth, 10 * 20m);
            });
        }

        private void AssertCompletion<T>(Task<T> task, T expectedValue)
        {
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            Assert.AreEqual(expectedValue, task.Result);
        }

        private void AssertFaulted<TException>(Task task) where TException : Exception
        {
            Assert.AreEqual(TaskStatus.Faulted, task.Status);
            Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
            Assert.AreEqual(typeof(TException), task.Exception.InnerExceptions[0].GetType());
        }
    }
}
