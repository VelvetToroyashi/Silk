using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Greetings;
using Silk.Errors;
using Silk.Services.Data;
using Silk.Shared.Types;

namespace Silk.Services.Guild;

public class GuildGreetingService : IHostedService
{
    private readonly List<PendingGreetingEntity> _pendingGreetings = new();

    private readonly AsyncTimer _timer;
    
    private readonly GuildConfigCacheService       _config;
    private readonly ILogger<GuildGreetingService> _logger;

    private readonly IMediator              _mediator;
    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestGuildAPI   _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;

    public GuildGreetingService
    (
        GuildConfigCacheService       config,
        ILogger<GuildGreetingService> logger,
        IMediator                     mediator,
        IDiscordRestUserAPI           userApi,
        IDiscordRestGuildAPI          guildApi,
        IDiscordRestChannelAPI        channelApi
    )
    {
        _config = config;
        _logger = logger;

        _mediator   = mediator;
        _userApi    = userApi;
        _guildApi   = guildApi;
        _channelApi = channelApi;
        
        //It's important to yield to the queue task here because if we get 429'd, 
        // Polly will retry the request, which will continue to block the queue task.
        _timer = new(QueueLoopAsync, TimeSpan.FromSeconds(5), true);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var greetings = await _mediator.Send(new GetPendingGreetings.Request(), cancellationToken);
        
        _pendingGreetings.AddRange(greetings);
        
        _timer.Start();
        
        _logger.LogInformation("Started greeting service.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        _timer.Dispose();
        _pendingGreetings.Clear();
        
        _logger.LogInformation("Stopped greeting service.");

        return Task.CompletedTask;
    }
    
    
    /// <summary>
    ///     Determines whether a member should be greeted on join, caching them otherwise.
    /// </summary>
    /// <param name="guildID">The ID of the guild.</param>
    /// <param name="user">The member that joined.</param>
    /// <param name="option">The option to use when checking if the member should be greeted.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    public async Task<Result> TryGreetMemberAsync(Snowflake guildID, IUser user)
    {
        var memberRes = await _guildApi.GetGuildMemberAsync(guildID, user.ID);

        if (!memberRes.IsDefined(out var member))
            return Result.FromError(memberRes.Error!); // This is guaranteed to be an error.

        var config = await _config.GetConfigAsync(guildID);

        if (!config.Greetings.Any())
            return Result.FromSuccess();

        if (config.Greetings.All(p => p.Option is GreetingOption.DoNotGreet))
            return Result.FromSuccess();

        //There could be multiple greetings, so we check each one.
        foreach (var greeting in config.Greetings)
        {
            if (greeting.Option is GreetingOption.DoNotGreet)
                continue;

            if (greeting.Option is GreetingOption.GreetOnJoin) // If we can greet immediately, don't make a db call.
            {
                var res = await GreetAsync(guildID, user.ID, greeting.ChannelID, greeting.Message);

                if (!res.IsSuccess)
                    return res;

                continue; // There may be multiple greetings, so we continue.
            }

            if (greeting.Option is GreetingOption.GreetOnRole)
            {
                var greetingResult = await _mediator.Send(new AddPendingGreeting.Request(user.ID, guildID, greeting.Id));

                if (greetingResult.IsSuccess)
                {
                    _pendingGreetings.Add(greetingResult.Entity);
                }
                else
                {
                    _logger.LogError("Failed to add greeting: {Error}", greetingResult.Error);
                }

                break;
            }

            _logger.LogError("Unhandled greeting option. {Option}, spawned from guild {Guild}", greeting.Option, guildID);
        }

        return Result.FromSuccess();
    }

    private async Task QueueLoopAsync()
    {
        if (!_pendingGreetings.Any())
            return;
        
        for (var i = _pendingGreetings.Count - 1; i >= 0; i--)
        {
            var pending      = _pendingGreetings[i];
            var memberResult = await _guildApi.GetGuildMemberAsync(pending.GuildID, pending.UserID);

            if (!memberResult.IsDefined(out var member))
            {
                _logger.LogError("Failed to get member {User} in guild {Guild}", pending.UserID, pending.GuildID);
                continue;
            }

            var guildConfig = await _config.GetConfigAsync(pending.GuildID);

            if (guildConfig.Greetings.FirstOrDefault(g => g.Id == pending.GreetingID) is not { } greeting)
            {
                _logger.LogError("A queued greeting is no longer present in the guild's config. ({Guild})", pending.GuildID);
                continue;
            }

            if (greeting.Option is GreetingOption.DoNotGreet)
                continue;
            
            if (member.Roles.All(r => r != greeting.MetadataID))
                continue;
            
            _pendingGreetings.RemoveAt(i);
            
            var res = await GreetAsync(pending.GuildID, pending.UserID, greeting.ChannelID, greeting.Message);
            
            if (!res.IsSuccess)
                _logger.LogError("Failed to greet {User} in guild {Guild}", pending.UserID, pending.GuildID);
        }
    }
    
    
    private async Task<Result> GreetAsync(Snowflake guildID, Snowflake memberID, Snowflake channelId, string greetingMessage)
    {
        string formattedMessage;

        var memberResult = await _userApi.GetUserAsync(memberID);

        if (!memberResult.IsDefined(out var member))
            return Result.FromError(memberResult.Error!);

        var permissionRes = await EnsurePermissionsAsync(guildID, channelId);

        if (!permissionRes.IsSuccess)
            return permissionRes;

        if (greetingMessage.Contains("{s}"))
        {
            var guildResult = await _guildApi.GetGuildAsync(guildID);

            if (!guildResult.IsDefined(out var guild))
                return Result.FromError(guildResult.Error!); //This checks `IsSuccess`, which implies the error isn't null

            formattedMessage = greetingMessage.Replace("{s}", guild.Name)
                                              .Replace("{u}", member.Username)
                                              .Replace("{@u}", $"<@{member.ID}>");
        }
        else
        {
            formattedMessage = greetingMessage.Replace("{u}", member.Username)
                                              .Replace("{@u}", $"<@{member.ID}>");
        }

        Result<IMessage> sendResult;

        if (formattedMessage.Length <= 2000)
        {
            sendResult = await _channelApi.CreateMessageAsync(channelId, formattedMessage);
        }
        else
        {
            var embed = new Embed(Colour: Color.FromArgb(47, 49, 54));

            sendResult = await _channelApi
               .CreateMessageAsync
                    (
                     channelID: channelId,
                     embeds: new[] { embed },
                     allowedMentions: new AllowedMentions
                     {
                         Parse = new[]
                         {
                             MentionType.Users,
                             MentionType.Roles
                         }
                     }
                    );
        }

        return sendResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(sendResult.Error);
    }

    /// <summary>
    /// Determines whether requisite permissions exist for a given channel.
    /// </summary>
    /// <param name="guildID">The ID of the guild the channel exists on.</param>
    /// <param name="channelID">The ID of the channel to check.</param>
    /// <returns>A successful result if the permissions are correct.</returns>
    private async Task<Result> EnsurePermissionsAsync(Snowflake guildID, Snowflake channelID)
    {
        var userResult = await _userApi.GetCurrentUserAsync();

        if (!userResult.IsDefined(out var user))
        {
            _logger.LogCritical("Unable to fetch current user.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch current user."));
        }

        var memberResult = await _guildApi.GetGuildMemberAsync(guildID, user.ID);

        if (!memberResult.IsDefined(out var member))
        {
            _logger.LogCritical("Unable to fetch current user's member.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch current member."));
        }

        var channelResult = await _channelApi.GetChannelAsync(channelID);

        if (!channelResult.IsDefined(out var channel))
        {
            _logger.LogCritical("Unable to fetch channel.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch channel."));
        }

        var rolesResult = await _guildApi.GetGuildRolesAsync(guildID);

        if (!rolesResult.IsDefined(out var roles))
        {
            _logger.LogCritical("Unable to fetch guild.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch roles."));
        }

        var permissions = DiscordPermissionSet
           .ComputePermissions
                (
                 user.ID,
                 roles.Single(r => r.ID == guildID),
                 roles.Where(r => member.Roles.Contains(r.ID)).ToArray(),
                 channel.PermissionOverwrites.Value
                );

        if (!permissions.HasPermission(DiscordPermission.SendMessages))
            return Result.FromError(new PermissionError($"I cannot send messages to the specified greeting channel ({channel.ID})."));

        if (!permissions.HasPermission(DiscordPermission.EmbedLinks))
            return Result.FromError(new PermissionError($"I cannot embed links in the specified greeting channel ({channel.ID})."));

        return Result.FromSuccess();
    }
    

}