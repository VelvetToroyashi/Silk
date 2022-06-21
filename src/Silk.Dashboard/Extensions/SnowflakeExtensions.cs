using Remora.Discord.API;
using Remora.Rest.Core;

namespace Silk.Dashboard.Extensions;

public static class SnowflakeExtensions
{
    public static T ToSnowflake<T>(this string value) 
        => DiscordSnowflake.TryParse(value, out var snowflake) 
            ? (T) (object) snowflake 
            : default;

    private static ulong? ToUlongHelper(Snowflake? snowflake)
        => snowflake?.Value ?? default;

    private static Snowflake? ToSnowflakeHelper(ulong? value)
        => value.HasValue ? DiscordSnowflake.New(value.Value) : default;

    public static ulong? ToUlong(this Snowflake? snowflake)
        => ToUlongHelper(snowflake);

    public static ulong ToUlong(this Snowflake snowflake)
        => ToUlongHelper(snowflake) ?? default;

    public static Snowflake? ToSnowflake(this ulong? value)
        => ToSnowflakeHelper(value);

    public static Snowflake ToSnowflake(this ulong value)
        => ToSnowflakeHelper(value) ?? default;
}