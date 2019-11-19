using Microsoft.Extensions.DependencyInjection;

namespace PIREventProcessor.Utilities
{
    public static class SetUp
    {
        public static void UseUtilities(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITimeProvider, TimeProvider>();
        }
    }
}