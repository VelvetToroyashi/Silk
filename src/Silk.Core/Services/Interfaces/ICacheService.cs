namespace Silk.Core.Services.Interfaces
{
    ///<summary>Interface for all cache-like services, to allow a specific guild's cache to be invalidated</summary>
    public interface ICacheService 
    {
        /// <summary>
        /// Requisite event handler method for <see cref="GuildConfigUpdated"/> event.
        /// </summary>
        /// <param name="id">The id of the guild that was updated.</param>
        public void PurgeCache(ulong id);
    }
    
}