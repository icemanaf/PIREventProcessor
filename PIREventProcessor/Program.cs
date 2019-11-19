using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PIREventProcessor.Influx;
using PIREventProcessor.Kafka;
using PIREventProcessor.Processor;
using PIREventProcessor.MessageActionFilters;
using PIREventProcessor.Utilities;
using Serilog;

namespace PIREventProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<App>().Run();
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

            services.AddTransient<App>();
        }
    }
}