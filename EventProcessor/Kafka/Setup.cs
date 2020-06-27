using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProcessor.Kafka
{
    public static class Setup
    {
        public static void  UseKafka(this IServiceCollection services,IConfiguration configuration)
        {

            services.AddTransient<IKafkaClient, KafkaClient>();
        }
    }
}
