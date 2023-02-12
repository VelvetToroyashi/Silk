using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mediator;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Services.Data;

public class GuildCacherService
{
    private readonly SemaphoreSlim _lock = new(1);
    
    /// <summary>
    /// The time in which a guild is considered new, if the joined timestamp is within this threshold.
    /// </summary>
    // If we joined within the last 30 seconds, we consider it new.
    // This is bound to change in the future. For now 30s is good enough to allow for caching,
    // as well as accommodating for any responder delays.
    private readonly TimeSpan _joinedTimestampThreshold = 30.Seconds();
    
    private const string GuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs.\n\n" +
                                                    $"I respond to mentions and `{StringConstants.DefaultCommandPrefix}` by default, "      +
                                                    $"but you can change that with `{StringConstants.DefaultCommandPrefix}prefix`\n\n"      +
                                                    $"Ex. `{StringConstants.DefaultCommandPrefix}prefix` `<new-prefix>` (make sure there's " +
                                                    "no space between the bot's prefix and the command you want to use!)\n\n"      +
                                                    "There's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!\n";
    
    private readonly IMediator              _mediator;
    private readonly IDiscordRestUserAPI    _users;
    private readonly IDiscordRestGuildAPI   _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;

    private readonly ILogger<GuildCacherService> _logger;
    
    private readonly IEmbed _onGuildJoinEmbed = new Embed(
                                                          Title: "Thank you for adding me!",
                                                          Colour: Color.CornflowerBlue,
                                                          Description: GuildJoinThankYouMessage
                                                         );
   

    /// <summary>
    ///     Required permissions to send a welcome message to a new guild.
    /// </summary>
    private readonly DiscordPermission _welcomeMessagePermissions = (DiscordPermission)((int)DiscordPermission.SendMessages | (int)DiscordPermission.EmbedLinks);

    public GuildCacherService
    (
        IMediator                   mediator,
        IDiscordRestUserAPI         users,
        IDiscordRestGuildAPI        guildApi,
        IDiscordRestChannelAPI      channelApi,
        ILogger<GuildCacherService> logger
    )
    {
        _mediator   = mediator;
        _users      = users;
        _guildApi   = guildApi;
        _channelApi = channelApi;
        _logger     = logger;
    }

    public async Task<Result> GreetGuildAsync(IGuildCreate.IAvailableGuild guild)
    {
        var currentUserResult = await _users.GetCurrentUserAsync();

        if (!currentUserResult.IsSuccess)
            return Result.FromError(currentUserResult.Error);

        var currentMemberResult = await _guildApi.GetGuildMemberAsync(guild.ID, currentUserResult.Entity.ID);

        if (!currentMemberResult.IsSuccess)
            return Result.FromError(currentMemberResult.Error);

        var currentUser   = currentUserResult.Entity;
        
        var currentMember = currentMemberResult.Entity;

        // check if we joined within the last 30 seconds
        if (currentMember.JoinedAt + _joinedTimestampThreshold < DateTimeOffset.Now)
            return Result.FromSuccess();
        
        var channels = guild.Channels;

        IChannel? availableChannel = null;
        
        _logger.LogTrace("Attempting to find open channel to send welcome message to");
        foreach (var channel in channels)
        {
            if (channel.Type is not ChannelType.GuildText)
                continue;

            var overwrites = channel.PermissionOverwrites.Value;

            var permissions = DiscordPermissionSet.ComputePermissions
            (
                 currentUser.ID,
                 guild.Roles.Single(r => r.ID == guild.ID),
                 guild.Roles.Where(r => currentMember.Roles.Contains(r.ID)).ToArray(),
                 overwrites
            );

            if (permissions.HasPermission(_welcomeMessagePermissions))
            {
                availableChannel = channel;
                
                _logger.LogDebug("Found available message channel");
                break;
            }
        }

        if (availableChannel is null)
        {
            _logger.LogWarning("Unable to find open channel to send thank you message to in guild {GuildID}", guild.ID);
        }
        else
        {
            var send = await _channelApi.CreateMessageAsync(availableChannel.ID, embeds: new[] { _onGuildJoinEmbed });

            if (send.IsSuccess)
            {
                _logger.LogInformation("Sent thank you message to new guild: {GuildID}", guild.ID);
                return Result.FromSuccess();
            }
        }

        return Result.FromSuccess();
    }
    
    public Task CacheGuildAsync(Snowflake guildID) => _mediator.Send(new AddGuild.Request(guildID, StringConstants.DefaultCommandPrefix)).AsTask();

}
