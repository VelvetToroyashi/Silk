using System;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net;

namespace Silk.Extensions.DSharpPlus
{
    public static class SnowflakeExtensions
    {
        public static DiscordClient GetClient(this SnowflakeObject snowflake)
        {
            Type type = snowflake.GetType() == typeof(SnowflakeObject) ? snowflake.GetType() : snowflake.GetType().BaseType!;

            var client = type
                .GetProperty("Discord", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(snowflake) as DiscordClient;
            return client!;
        }

        public static DiscordApiClient GetApiClient(this DiscordClient client)
        {
            var api = typeof(DiscordClient)
                .GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(client) as DiscordApiClient;
            return api!;
        }
        
    }
}