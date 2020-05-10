using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProcessor.MessageActionFilters
{
    public static class SetUp
    {
        public static void UseMessageActionFilters(this IServiceCollection services, IConfiguration configuration)
        {
            //setup configuration
            services.Configure<StationConfig>(configuration.GetSection("StationConfig"));

            services.AddTransient<PIRDetectFilter>();
        }
    }
}