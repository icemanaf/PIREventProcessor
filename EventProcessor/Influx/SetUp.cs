using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProcessor.Influx
{
    public static class SetUp
    {
        public static void UseInflux(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IInfluxClient, InfluxClient>();

            services.Configure<InfluxConfig>(configuration.GetSection("influx"));
        }
    }
}