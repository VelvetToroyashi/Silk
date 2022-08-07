using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Moq; 
using NUnit.Framework;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Services.Data;
using Silk.Services.Guild;
using Silk.Services.Interfaces;

namespace Silk.Tests.Services;

public class RaidHelperTests
{
    private static readonly ImmutableArray<Snowflake> MockIDs = Enumerable.Range(1, 20).Select(x => DiscordSnowflake.New((ulong)x)).ToImmutableArray();

    private static readonly ImmutableArray<Snowflake> MockOldIDs = new[] { 444881658809024532, 755254361533186092, 936459828044374036 }.Select(x => DiscordSnowflake.New((ulong)x)).ToImmutableArray();

    private static readonly ImmutableArray<(Snowflake ChannelID, Snowflake AuthorID, Snowflake MessageID, string Content)> MockMessages = new[]
    {
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(3), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(4), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(5), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(6), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(7), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(3), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "This is a raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(1), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "Oh no! A raid!!"),
        (ChannelID: DiscordSnowflake.New(2), AuthorID: DiscordSnowflake.New(9), MessageID: Snowflake.CreateTimestampSnowflake(DateTimeOffset.UtcNow.AddMinutes(10)), Content: "Oh no, a raid!!")
    }.ToImmutableArray();
    
    private static readonly Snowflake[] ExpectedLegitIDs = new[] { DiscordSnowflake.New(1), DiscordSnowflake.New(9) };
    private const           string      DummyToken       = "dummy-token";
    private static readonly Snowflake   DummyGuild       = new Snowflake(123456789012345678);


    [Test]
    public async Task JoinVelocityThresholdIsCaught()
    {
        var infractions = new Mock<IInfractionService>();
        var users       = new Mock<IDiscordRestUserAPI>();
        var cache       = new Mock<GuildConfigCacheService>(Mock.Of<IMemoryCache>(), Mock.Of<IMediator>());

        users.Setup(u => u.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User(default, "", default, null));
        
        cache.Setup(c => c.GetConfigAsync(It.IsAny<Snowflake>()))
             .ReturnsAsync(new GuildConfigEntity
                               { EnableRaidDetection = true, RaidDetectionThreshold = 3, RaidCooldownSeconds = 120});

        var raid = new RaidDetectionService(infractions.Object, users.Object, cache.Object);

        await raid.StartAsync(CancellationToken.None);

        foreach (var mockEvent in MockIDs)
            await raid.HandleJoinAsync(DummyGuild, mockEvent, DateTimeOffset.UtcNow, false);
        
        await raid.StopAsync(CancellationToken.None);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.AtLeastOnce());
    }
    
    [Test]
    public async Task JoinVelocityBurstsIsCaught()
    {
        var infractions = new Mock<IInfractionService>();
        var users       = new Mock<IDiscordRestUserAPI>();
        var cache       = new Mock<GuildConfigCacheService>(Mock.Of<IMemoryCache>(), Mock.Of<IMediator>());

        users.Setup(u => u.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User(default, "", default, null));
        
        cache.Setup(c => c.GetConfigAsync(It.IsAny<Snowflake>()))
             .ReturnsAsync(new GuildConfigEntity
                               { EnableRaidDetection = true, RaidDetectionThreshold = 3, RaidCooldownSeconds = 120});

        var raid = new RaidDetectionService(infractions.Object, users.Object, cache.Object);

        await raid.StartAsync(CancellationToken.None);

        foreach (var mockEvent in MockIDs.Take(10))
            await raid.HandleJoinAsync(DummyGuild, mockEvent, DateTimeOffset.UtcNow, false);
        
        foreach (var mockEvent in MockIDs.Skip(10))
            await raid.HandleJoinAsync(DummyGuild, mockEvent, DateTimeOffset.UtcNow, false);
        
        await raid.StopAsync(CancellationToken.None);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.AtLeastOnce());
    }
    
    [Test]
    public async Task AccountCreationThresholdIsCaught()
    {
        var infractions = new Mock<IInfractionService>();
        var users       = new Mock<IDiscordRestUserAPI>();
        var cache       = new Mock<GuildConfigCacheService>(Mock.Of<IMemoryCache>(), Mock.Of<IMediator>());
        
        users.Setup(u => u.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User(default, "", default, null));
        
        cache.Setup(c => c.GetConfigAsync(It.IsAny<Snowflake>()))
             .ReturnsAsync(new GuildConfigEntity
                               { EnableRaidDetection = true, RaidDetectionThreshold = 3, RaidCooldownSeconds = 0 });
        
        var raid = new RaidDetectionService(infractions.Object, users.Object, cache.Object);
        
        await raid.StartAsync(CancellationToken.None);
        
        foreach (var mockEvent in MockIDs)
            await raid.HandleJoinAsync(DummyGuild, mockEvent, DateTimeOffset.UtcNow, false);
        
        await raid.StopAsync(CancellationToken.None);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.AtLeastOnce());
    }

    [Test]
    public async Task AccountCreationTHresholdAllowsLegitimateUsers()
    {
        var infractions = new Mock<IInfractionService>();
        var users       = new Mock<IDiscordRestUserAPI>();
        var cache       = new Mock<GuildConfigCacheService>(Mock.Of<IMemoryCache>(), Mock.Of<IMediator>());
        
        users.Setup(u => u.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User(default, "", default, null));
        
        // -1 to skip velocity check
        cache.Setup(c => c.GetConfigAsync(It.IsAny<Snowflake>()))
             .ReturnsAsync(new GuildConfigEntity
                               { EnableRaidDetection = true, RaidDetectionThreshold = 3, RaidCooldownSeconds = -1 });
        
        var raid = new RaidDetectionService(infractions.Object, users.Object, cache.Object);
        
        await raid.StartAsync(CancellationToken.None);
        
        foreach (var mockEvent in MockIDs.Take(3).Concat(MockOldIDs))
            await raid.HandleJoinAsync(DummyGuild, mockEvent, DateTimeOffset.UtcNow, false);
        
        await raid.StopAsync(CancellationToken.None);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsNotIn(MockOldIDs.AsEnumerable()), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.AtLeastOnce);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsIn(MockOldIDs.AsEnumerable()), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Never);
    }
    
    [Test]
    public async Task MessageDetectionCatchesSpammedMessages()
    {
        var infractions = new Mock<IInfractionService>();
        var users       = new Mock<IDiscordRestUserAPI>();
        var cache       = new Mock<GuildConfigCacheService>(Mock.Of<IMemoryCache>(), Mock.Of<IMediator>());
        
        users.Setup(u => u.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User(default, "", default, null));
        
        // -1 to skip velocity check
        cache.Setup(c => c.GetConfigAsync(It.IsAny<Snowflake>()))
             .ReturnsAsync(new GuildConfigEntity
                               { EnableRaidDetection = true, RaidDetectionThreshold = 3, RaidCooldownSeconds = -1 });
        
        var raid = new RaidDetectionService(infractions.Object, users.Object, cache.Object);
        
        await raid.StartAsync(CancellationToken.None);

        foreach (var mockEvent in MockMessages)
            await raid.HandleMessageAsync(DummyGuild, mockEvent.ChannelID, mockEvent.MessageID, mockEvent.AuthorID, mockEvent.Content);
        
        await raid.StopAsync(CancellationToken.None);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsNotIn(ExpectedLegitIDs), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.AtLeastOnce);
        
        infractions.Verify(i => i.BanAsync(It.IsAny<Snowflake>(), It.IsIn(ExpectedLegitIDs), It.IsAny<Snowflake>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Never);
    }
}