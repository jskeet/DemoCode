// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Cloud.Functions.Hosting;
using Google.Cloud.Functions.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Testing;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ZoneClock.Tests
{
    [FunctionsStartup(typeof(FakeClockStartup))]
    public class FunctionTest : FunctionTestBase<Function>
    {
        [SetUp]
        public void ClearLogs() => Server.ClearLogs();

        [Test]
        public async Task NoCustomZones()
        {
            string actualText = await ExecuteHttpGetRequestAsync();
            string expectedText = "Current time in UTC: 2015-06-03T20:25:30\n";
            Assert.AreEqual(expectedText, actualText);
            Assert.IsEmpty(GetFunctionLogEntries());
        }

        [Test]
        public async Task ValidCustomZones()
        {
            string actualText = await ExecuteHttpGetRequestAsync("?zone=Europe/London&zone=America/New_York");
            string[] expectedLines =
            {
                "Current time in UTC: 2015-06-03T20:25:30",
                "Current time in Europe/London: 2015-06-03T21:25:30",
                "Current time in America/New_York: 2015-06-03T16:25:30"
            };
            var actualLines = actualText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expectedLines, actualLines);
            Assert.IsEmpty(GetFunctionLogEntries());
        }

        [Test]
        public async Task InvalidCustomZoneIsIgnoredButLogged()
        {
            string actualText = await ExecuteHttpGetRequestAsync("?zone=America/Metropolis&zone=Europe/London");
            // We still print UTC and Europe/London, but America/Metropolis isn't mentioned at all.
            string[] expectedLines =
            {
                "Current time in UTC: 2015-06-03T20:25:30",
                "Current time in Europe/London: 2015-06-03T21:25:30"
            };
            var actualLines = actualText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expectedLines, actualLines);

            var logEntries = GetFunctionLogEntries();
            Assert.AreEqual(1, logEntries.Count);
            var logEntry = logEntries[0];
            Assert.AreEqual(LogLevel.Warning, logEntry.Level);
            StringAssert.Contains("America/Metropolis", logEntry.Message);
        }

        private class FakeClockStartup : FunctionsStartup
        {
            public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
                services.AddSingleton<IClock>(new FakeClock(Instant.FromUtc(2015, 6, 3, 20, 25, 30)));
        }
    }
}
