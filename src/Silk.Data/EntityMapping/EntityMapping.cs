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
        TypeAdapterConfig<GuildGreetingEntity, GuildGreetingDTO>
           .NewConfig()
            ;

        TypeAdapterConfig<GuildGreetingDTO, GuildGreetingEntity>
           .NewConfig()
           .Ignore(dest => dest.Guild)
            ;

        // Currently 1-1 mapping
        TypeAdapterConfig<ExemptionEntity, ExemptionDTO>
           .NewConfig()
           .TwoWays()
            ;

        // Currently 1-1 mapping
        TypeAdapterConfig<InviteEntity, InviteDTO>
           .NewConfig()
           .TwoWays()
            ;

        // Currently 1-1 mapping
        TypeAdapterConfig<LoggingChannelEntity, LoggingChannelDTO>
           .NewConfig()
           .TwoWays()
            ;

        // Currently 1-1 mapping
        TypeAdapterConfig<InfractionStepEntity, InfractionStepDTO>
           .NewConfig()
           .TwoWays()
            ;

        // Currently 1-1 mapping
        TypeAdapterConfig<GuildLoggingConfigEntity, GuildLoggingConfigDTO>
           .NewConfig()
           .TwoWays()
            ;

        TypeAdapterConfig<InviteConfigEntity, InviteConfigDTO>
           .NewConfig()
           .Map(dest => dest.GuildConfigId, src => src.GuildModConfigId)
            ;

        TypeAdapterConfig<InviteConfigDTO, InviteConfigEntity>
           .NewConfig()
           .Ignore(dest => dest.GuildConfig)
           .Map(dest => dest.GuildModConfigId, src => src.GuildConfigId)
            ;

        TypeAdapterConfig<GuildConfigEntity, GuildConfigDTO>
           .NewConfig()
            ;

        TypeAdapterConfig<GuildConfigDTO, GuildConfigEntity>
           .NewConfig()
           .Ignore(dest => dest.Guild)
            ;
    }
}