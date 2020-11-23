using Microsoft.Extensions.Configuration;

namespace SilkBot.Extensions
{
    public static class MiscExtensions
    {
        public static T Get<T>(this IConfiguration config, string key) where T : new()
        {
            var instance = new T();
            config.GetSection(key).Bind(instance);
            return instance;
        }
    }
}