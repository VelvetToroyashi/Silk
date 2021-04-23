using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk.Extensions.DSharpPlus
{
    public static class SnowflakeExtensions
    {
        public static DiscordClient GetClient(this SnowflakeObject snowflake)
        {
            var type = snowflake.GetType() == typeof(SnowflakeObject) ? snowflake.GetType() : snowflake.GetType().BaseType!;

            var client = type
                .GetProperty("Discord", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(snowflake) as DiscordClient;
            return client!;
        }
    }
}