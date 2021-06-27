using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net;

namespace Silk.Extensions.DSharpPlus
{
    public static class SnowflakeExtensions
    {
        public static IEnumerable<TType> OfType<T, TType>(this IEnumerable<TType> collection, T type) where TType : SnowflakeObject where T : Enum
        {
            var snowflakeObjects = collection as SnowflakeObject[] ?? collection.ToArray();
            
            if (!snowflakeObjects.Any())
                yield break;

            var prop = snowflakeObjects.First().GetType().GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);

            if (prop is null)
                throw new ArgumentException("Snowflake object in collection does not contain a 'Type' property to check.");
            
            foreach (var entity in snowflakeObjects)
            {
                var value = (T)prop.GetValue(entity)!;

                if (value.Equals(type))
                    yield return (entity as TType)!;
            }
        }
        
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