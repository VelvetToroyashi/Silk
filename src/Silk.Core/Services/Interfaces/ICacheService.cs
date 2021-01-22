using System.Threading.Tasks;

namespace Silk.Core.Services.Interfaces
{
    ///<summary>Interface for all cache-like services, to allow a specific guild's cache to be invalidated</summary>
    public interface ICacheService 
    {
        ///<summary>Invalidate the config of a specific guild. </summary>
        public Task InvalidateAsync(ulong guildId);
    }
    
}