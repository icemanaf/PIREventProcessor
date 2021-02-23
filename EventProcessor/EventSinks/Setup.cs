using EventProcessor.EventSinks.PIR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proto.Models;

namespace EventProcessor.EventSinks
{
    public static class SetUp
    {
        public static void UseMessageActionFilters(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PIRDetectionSinkConfig>(configuration.GetSection("PIRDetectionSinkConfig"));
            //setup configuration
            services.Configure<StationConfig>(configuration.GetSection("StationConfig"));

            services.Configure<AckSinkConfig>(configuration.GetSection("AckSinkConfig"))

            services.AddSingleton<IEventSink<KafkaMessage>, PIRDetectionSink>();

            services.AddSingleton<IEventSink<KafkaMessage>, AckSink>();
        }
    }
}