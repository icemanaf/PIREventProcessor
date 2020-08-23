using EventProcessor.MessageActionFilters.PIR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proto.Models;

namespace EventProcessor.MessageActionFilters
{
    public static class SetUp
    {
        public static void UseMessageActionFilters(this IServiceCollection services, IConfiguration configuration)
        {
            //setup configuration
            services.Configure<StationConfig>(configuration.GetSection("StationConfig"));

            services.Configure<PIRDetectionFilter>(configuration.GetSection("PIRDetectionFilterConfig"));

            services.AddSingleton<IMessageActionFilter<KafkaMessage>, PIRDetectionFilter>();

            services.AddSingleton<IMessageActionFilter<KafkaMessage>, AckFilter>();
        }
    }
}