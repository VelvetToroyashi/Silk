using Mapster;
using Silk.Data.DTOs.Guilds.Config;
using Silk.Data.Entities;
using Silk.Data.Entities.Guild.Config;

namespace Silk.Data.EntityMapping;

// Most likely move mapping to another common library
public static class EntityMapping
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig<GuildGreetingEntity, GuildGreeting>
           .NewConfig();

        TypeAdapterConfig<GuildGreeting, GuildGreetingEntity>
           .NewConfig()
           .Ignore(dest => dest.Guild);

        TypeAdapterConfig<ExemptionEntity, Exemption>
           .NewConfig()
           .Map(dest => dest.Coverage, src => src.Exemption);

        TypeAdapterConfig<Exemption, ExemptionEntity>
           .NewConfig()
           .Map(dest => dest.Exemption, src => src.Coverage);

        // Currently 1-1 mapping
        TypeAdapterConfig<InviteEntity, Invite>
           .NewConfig()
           .TwoWays();

        // Currently 1-1 mapping
        TypeAdapterConfig<LoggingChannelEntity, LoggingChannel>
           .NewConfig()
           .TwoWays();

        // Currently 1-1 mapping
        TypeAdapterConfig<InfractionStepEntity, InfractionStep>
           .NewConfig()
           .TwoWays();

        // Currently 1-1 mapping
        TypeAdapterConfig<GuildLoggingConfigEntity, GuildLoggingConfig>
           .NewConfig()
           .TwoWays();

        TypeAdapterConfig<InviteConfigEntity, InviteConfig>
           .NewConfig()
           .Map(dest => dest.GuildConfigId, src => src.GuildModConfigId);

        TypeAdapterConfig<InviteConfig, InviteConfigEntity>
           .NewConfig()
           .Ignore(dest => dest.GuildConfig)
           .Map(dest => dest.GuildModConfigId, src => src.GuildConfigId);

        TypeAdapterConfig<GuildConfigEntity, GuildConfig>
           .NewConfig();

        TypeAdapterConfig<GuildConfig, GuildConfigEntity>
           .NewConfig()
           .Ignore(dest => dest.Guild);
    }
}