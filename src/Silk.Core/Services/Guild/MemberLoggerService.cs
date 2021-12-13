using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Services.Bot;
using Silk.Core.Services.Data;
using Silk.Extensions;

namespace Silk.Core.Services.Server;

/// <summary>
/// A service for logging members joining and leaving.
/// </summary>
public class MemberLoggerService
{
    private readonly IMediator               _mediator;
    private readonly GuildConfigCacheService _configService;
    private readonly ChannelLoggingService   _channelLogger;
    
    public MemberLoggerService(IMediator mediator, GuildConfigCacheService configService, ChannelLoggingService channelLogger)
    {
        _mediator           = mediator;
        _configService      = configService;
        _channelLogger = channelLogger;
    }

    public async Task<Result> LogMemberJoinAsync(Snowflake guildID, IGuildMember member)
    {
        if (!member.User.IsDefined(out var user))
            return Result.FromSuccess();
        
        var config = await _configService.GetModConfigAsync(guildID);
        
        if (!config.LoggingConfig.LogMemberJoins)
            return Result.FromSuccess();

        var channel = config.LoggingConfig.MemberJoins;
        
        if (channel is null)
            return Result.FromSuccess();

        var twoWeeksOld = user.ID.Timestamp.AddDays(14) > DateTimeOffset.UtcNow;
        var twoDaysOld = user.ID.Timestamp.AddDays(2) > DateTimeOffset.UtcNow;

        var userResult = await _mediator.Send(new GetOrCreateUserRequest(guildID, user.ID, null, member.JoinedAt));

        var userFields = new List<EmbedField>()
        {
            new("Username:", $"{user.Username}#{user.Discriminator}", true),
            new("User ID:", user.ID.ToString(), true),
            new("User Created:", $"<t:{user.ID.Timestamp.ToUnixTimeSeconds()}:F>", true),
        };
        
        var sb = new StringBuilder();
        
        if (!userResult.IsDefined(out var userData))
            return Result.FromError(userResult.Error!);

        if (userData.Infractions.Any())
        {
            sb.AppendLine("User has infractions on record");
            userFields.Add(new("Infractions:", userData
                                              .Infractions
                                              .GroupBy(inf => inf.Type)
                                              .Select(inf => $"{inf.Key}: {inf.Count()} time(s)")
                                              .Join("\n"), true));
        }
        
        if (twoDaysOld)
            sb.AppendLine("Account is only 2 days old");
        else if (twoWeeksOld)
            sb.AppendLine("Account is only 2 weeks old");
        
        var embed = new Embed()
        {
            Title       = "Member Joined",
            Description = sb.Length > 0 ? sb.ToString() : string.Empty,
            Colour      = twoDaysOld ? Color.DarkRed : twoWeeksOld ? Color.Orange : Color.SeaGreen,
            Fields      = userFields.ToArray()
        };
        
        return await _channelLogger.LogAsync(config.LoggingConfig.UseWebhookLogging, channel, embed);
    }
}