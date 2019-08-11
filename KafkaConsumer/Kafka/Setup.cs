using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PIREventProcessor.Kafka
{
    public static class Setup
    {
        public static void  UseKafka(this IServiceCollection services,IConfiguration configuration)
        {
            services.Configure<KafkaConfig>(configuration.GetSection("kafka"));

            services.AddTransient<IKafkaClient, KafkaClient>();
        }
    }
}
