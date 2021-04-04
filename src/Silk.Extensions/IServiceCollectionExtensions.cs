#region

using System;

#endregion

namespace Silk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static T? Get<T>(this IServiceProvider provider)
        {
            _ = provider ?? throw new ArgumentNullException($"{nameof(provider)} is not initialized!", new NullReferenceException());
            return (T?) provider.GetService(typeof(T))!;
        }
    }
}