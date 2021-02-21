#region

using System;
using System.Threading.Tasks;
using DSharpPlus;

#endregion

namespace Silk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static T Get<T>(this IServiceProvider provider)
        {
            _ = provider ?? throw new ArgumentNullException($"{nameof(provider)} is not initialized!", new NullReferenceException());
            return (T) provider.GetService(typeof(T));
        }


        public static async Task RegisterServices(this IServiceProvider provider, DiscordShardedClient c)
        {
            foreach (var client in c.ShardClients.Values) { }
        }
    }
}