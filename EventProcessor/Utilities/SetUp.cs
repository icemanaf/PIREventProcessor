using Microsoft.Extensions.DependencyInjection;

namespace EventProcessor.Utilities
{
    public static class SetUp
    {
        public static void UseUtilities(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITimeProvider, TimeProvider>();
        }
    }
}