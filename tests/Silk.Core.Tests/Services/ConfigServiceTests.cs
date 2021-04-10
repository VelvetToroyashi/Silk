using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Silk.Core.Data.MediatR.Unified.Guilds;
using Silk.Core.Discord.Services;
using Xunit;

namespace Silk.Core.Tests.Services
{
    public class ConfigServiceTests
    {
        private readonly Mock<IMemoryCache> _cache;
        private readonly ConfigService _configService;
        private readonly Mock<IMediator> _mediator;

        public ConfigServiceTests()
        {

            _cache = new();
            _cache.Setup(cache => cache.CreateEntry(0ul)).Returns(Mock.Of<ICacheEntry>);
            _mediator = new() {CallBase = false};


            _mediator
                .Setup(m => m.Send(It.IsAny<IRequest<GetGuildConfigRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(It.IsAny<GetGuildConfigRequest>())
                .Verifiable("uHHHH");

            _configService = new(_cache.Object, _mediator.Object);
        }

        [Fact]
        public async Task GetConfigAsync_WhenInvalidId_RetrievesFromDatabase()
        {
            //Act
            object discard;
            _cache.Setup(cache => cache.TryGetValue(0ul, out discard)).Returns(false);
            await _configService.GetConfigAsync(0);
            //Assert
            _mediator.Verify(x => x.Send(It.IsAny<GetGuildConfigRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConfigAsync_WhenValidId_RetrievesFromCache()
        {
            object discard;
            _cache.Setup(cache => cache.TryGetValue(0ul, out discard)).Returns(true);
            _mediator.Verify(m => m.Send(new(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}