using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProcessor.Processor
{
    public static class SetUp
    {
        public static void UseMessageProcessor(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IMessageStreamer<Proto.Models.KafkaMessage>, KMMessageStreamer>();
        }
    }
}