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
            services.Configure<PIRDetectionFilterConfig>(configuration.GetSection("PIRDetectionFilterConfig"));
            //setup configuration
            services.Configure<StationConfig>(configuration.GetSection("StationConfig"));

            services.AddSingleton<IMessageActionFilter<KafkaMessage>, PIRDetectionFilter>();

            services.AddSingleton<IMessageActionFilter<KafkaMessage>, AckFilter>();
        }
    }
}