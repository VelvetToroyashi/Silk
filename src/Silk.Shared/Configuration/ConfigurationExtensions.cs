using Microsoft.Extensions.Configuration;

namespace Silk.Shared.Configuration;

public static class ConfigurationExtensions
{
    /// <summary>
    ///     An extension method to get a <see cref="SilkConfigurationOptions" /> instance from the Configuration by Section Key
    /// </summary>
    /// <param name="config">the configuration</param>
    /// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
    public static SilkConfigurationOptions GetSilkConfigurationOptions(this IConfiguration config)
        => config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
}