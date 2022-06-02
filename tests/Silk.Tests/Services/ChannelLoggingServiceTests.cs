using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Services.Bot;

namespace Silk.Tests.Services;

public class ChannelLoggingServiceTests
{
    //TODO: More exhaustive tests
    
    private const string LoremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. "              +
        "Donec euismod, nisl eget consectetur tempor, nisl nunc ultrices eros, " +
        "eu porttitor nunc nisl eu nisl. Nulla facilisi.";
    
    private readonly LoggingChannelEntity loggingData = new LoggingChannelEntity
    {
        GuildID      = new(0),
        ChannelID    = new(0),
        WebhookID    = new(0),
        WebhookToken = "token"
    };
    
    [Test]
    public async Task SuccessfullyUsesWebhookLogging()
    {
        // Arrange
        var webhookAPIMock = new Mock<IDiscordRestWebhookAPI>();
        var channelLogger  = new ChannelLoggingService(default!, webhookAPIMock.Object, NullLogger<ChannelLoggingService>.Instance);
        
        // Act
        await channelLogger.LogAsync(true, loggingData, LoremIpsum, null, null);
        
        // Assert
        webhookAPIMock.Verify(x => x.ExecuteWebhookAsync(
                                                         loggingData.WebhookID, 
                                                         loggingData.WebhookToken,
                                                         It.IsAny<Optional<bool>>(),
                                                         It.IsAny<Optional<string>>(),
                                                         It.IsAny<Optional<string>>(),
                                                         default, default,
                                                         default, default, default,
                                                         default, default, default, default), Times.Once);

    }
    
    [Test]
    public async Task SuccessfullyUsesChannelLogging()
    {
        // Arrange
        var channelAPIMock = new Mock<IDiscordRestChannelAPI>();
        var channelLogger  = new ChannelLoggingService(channelAPIMock.Object, default!, NullLogger<ChannelLoggingService>.Instance);

        // Act
        await channelLogger.LogAsync(false, loggingData, LoremIpsum, null, null);

        // Assert
        channelAPIMock.Verify(x => x.CreateMessageAsync(
                                                        loggingData.ChannelID,
                                                        It.IsAny<Optional<string>>(),
                                                        default, default, default,
                                                        default, default, default,
                                                        default, default, default, default), Times.Once);
    }
}