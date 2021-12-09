using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Services.Data;

namespace Silk.Core.Tests.Services;

public class ConfigServiceTests
{
    private readonly Mock<IMemoryCache> _cache;
    private readonly GuildConfigCacheService      _guildConfigCacheService;
    private readonly Mock<IMediator>    _mediator;

    public ConfigServiceTests()
    {

        _cache = new();
        _cache.Setup(cache => cache.CreateEntry(0ul)).Returns(Mock.Of<ICacheEntry>);
        _mediator = new() { CallBase = false };


        _mediator
            .Setup(m => m.Send(It.IsAny<IRequest<GetGuildConfigRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<GetGuildConfigRequest>())
            .Verifiable("uHHHH");

        _guildConfigCacheService = new(_cache.Object, _mediator.Object);
    }

    [Test]
    public async Task GetConfigAsync_WhenInvalidId_RetrievesFromDatabase()
    {
        //Act
        object discard;
        _cache.Setup(cache => cache.TryGetValue(0ul, out discard)).Returns(false);
        await _guildConfigCacheService.GetConfigAsync(0);
        //Assert
        _mediator.Verify(x => x.Send(It.IsAny<GetGuildConfigRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetConfigAsync_WhenValidId_RetrievesFromCache()
    {
        object discard;
        _cache.Setup(cache => cache.TryGetValue(0ul, out discard)).Returns(true);
        _mediator.Verify(m => m.Send(new(), It.IsAny<CancellationToken>()), Times.Never);
    }
}