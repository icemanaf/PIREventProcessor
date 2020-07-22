using EventProcessor.Influx;
using EventProcessor.Kafka;
using EventProcessor.MessageActionFilters;
using EventProcessor.Processor;
using EventProcessor.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;

namespace EventProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var service = serviceProvider.GetService<App>();

            service.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false).Build();

            var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            services.AddLogging(builder => { builder.AddSerilog(logger, true); });

            services.UseKafka(configuration);

            services.UseInflux(configuration);

            services.UseMessageProcessor(configuration);

            services.UseMessageActionFilters(configuration);

            services.UseUtilities();

            services.AddSingleton<App>();

            services.Configure<AppConfig>(configuration.GetSection("app"));

            services.AddOptions();
        }
    }
}