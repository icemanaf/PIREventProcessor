using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PIREventProcessor.Processor
{
    public static class SetUp
    {
        public static void UseMessageProcessor(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IMessageProcessor, MessageProcessor>();
        }
    }
}