// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;
using System.Threading.Tasks;

namespace ZoneClock
{
    // Startup class to configure dependency injection (in this case).
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton<IClock>(SystemClock.Instance);
    }

    // Register the startup class; this attribute can also be applied to the assembly.
    [FunctionsStartup(typeof(Startup))]
    public class Function : IHttpFunction
    {
        private readonly IClock clock;
        private readonly ILogger logger;

        // Receive and remember the dependencies.
        public Function(IClock clock, ILogger<Function> logger) =>
            (this.clock, this.logger) = (clock, logger);

        public async Task HandleAsync(HttpContext context)
        {
            // Get the current instant in time via the clock.
            Instant now = clock.GetCurrentInstant();

            // Always write out UTC.
            await WriteTimeInZone(DateTimeZone.Utc);

            // Write out the current time in as many zones as the user has specified.
            foreach (var zoneId in context.Request.Query["zone"])
            {
                var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(zoneId);
                if (zone is null)
                {
                    logger.LogWarning("User provided invalid time zone '{id}'", zoneId);
                }
                else
                {
                    await WriteTimeInZone(zone);
                }
            }

            Task WriteTimeInZone(DateTimeZone zone)
            {
                string time = LocalDateTimePattern.GeneralIso.Format(now.InZone(zone).LocalDateTime);
                return context.Response.WriteAsync($"Current time in {zone.Id}: {time}\n");
            }
        }
    }
}
