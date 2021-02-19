using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Xunit;

namespace Silk.Core.Tests.Services
{
    public class ConfigServiceTests
    {
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<IDatabaseService> _db;
        private readonly ConfigService _configService;

        public ConfigServiceTests()
        {

            _cache = new();
            _cache.Setup(cache => cache.CreateEntry(0ul)).Returns(Mock.Of<ICacheEntry>);
            _db = new();
            _configService = new(_cache.Object, _db.Object);
        }

        [Fact]
        public async Task GetConfigAsync_WhenInvalidId_RetrievesFromDatabase()
        {
            //Act
            await _configService.GetConfigAsync(0);
            //Assert
            _db.Verify(db => db.GetConfigAsync(0), Times.Once);
        }

        [Fact]
        public async Task GetConfigAsync_WhenValidId_RetrievesFromCache()
        {
            object discard;
            _cache.Setup(cache => cache.TryGetValue(0ul, out discard)).Returns(true);
            _db.Verify(db => db.GetConfigAsync(0), Times.Never);
        }
    }
}