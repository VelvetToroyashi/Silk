using System.Threading.Tasks;

namespace Silk.Core.Services.Interfaces
{
    public interface IConfiguredService
    {
        /// <summary>
        /// True or false value set by <see cref="Configure"/> indicating if the service has been configued already.
        /// </summary>
        public bool HasConfigured { get; }

        /// <summary>
        /// Method called during startup to configure the service asynchronously.
        /// </summary>
        public Task Configure();
    }
}