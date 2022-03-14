using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginLoader.Unity
{
    internal static class Events
    {
        public static EventId Plugin = new(5, "Plugin");
    }

    /// <summary>
    ///     Extension methods for the plugin loader library.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Registers all required plugin loader dependencies in a <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns></returns>
        public static IServiceCollection RegisterShardedPluginServices(this IServiceCollection services) =>
            services
               .TryRegisterSingleton<PluginLoader>()
               .TryRegisterSingleton<PluginWatchdog>()
               .AddSingleton<IPluginLoaderService, PluginLoaderService>();

        private static IServiceCollection TryRegisterSingleton<T>(this IServiceCollection services) where T : class
        {
            try { services.AddSingleton<T>(); }
            catch { }
            return services;
        }
    }
}