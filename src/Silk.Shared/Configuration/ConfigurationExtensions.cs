using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silk.Shared.Configuration;

public static class ConfigurationExtensions
{
    /// <summary>
    /// An extension method to get a <see cref="SilkConfigurationOptions" /> instance from the
    /// Configuration by Section Key
    /// </summary>
    /// <param name="config">the configuration</param>
    /// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
    public static SilkConfigurationOptions GetSilkConfigurationOptions(this IConfiguration config)
        => config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();

    /// <summary>
    /// An extension method to add and bind <see cref="SilkConfigurationOptions" /> to IOptions
    /// configuration for appSettings.json and UserSecrets configuration.
    /// </summary>
    /// <param name="services">the service collection to use for configuration</param>
    /// <param name="configuration">the configuration used to get the configuration options section from</param>
    /// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options"/>
    public static IServiceCollection AddSilkConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        => services.Configure<SilkConfigurationOptions>(configuration.GetSection(SilkConfigurationOptions.SectionKey));
}