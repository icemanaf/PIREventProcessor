using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EventProcessor.MessageActionFilters.PIR
{
    public static class SetUp
    {
        public static void UsePIRFilters(this IServiceCollection services, IConfiguration configuration)
        {
            //setup configuration
            services.Configure<StationConfig>(configuration.GetSection("StationConfig"));

            services.AddTransient<PIRDetectFilter>();
        }
    }
}
