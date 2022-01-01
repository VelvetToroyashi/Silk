using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Interfaces;

namespace Silk.Services.Guild;

/// <summary>
/// A service for logging members joining and leaving.
/// </summary>
public class MemberLoggerService
{
    private readonly IMediator               _mediator;
    private readonly GuildConfigCacheService _configService;
    private readonly IChannelLoggingService   _channelLogger;
    
    public MemberLoggerService(IMediator mediator, GuildConfigCacheService configService, IChannelLoggingService channelLogger)
    {
        _mediator      = mediator;
        _configService = configService;
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
            new("Username:", user.ToDiscordTag(), true),
            new("User ID:", user.ID.ToString(), true),
            new("User Created:", user.ID.Timestamp.ToTimestamp(TimestampFormat.LongDateTime), true),
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

        
        var userInfractionJoinBuffer = userData.Infractions.Count(inf => inf.Type is
                                                                      InfractionType.Kick or
                                                                      InfractionType.Ban or
                                                                      InfractionType.SoftBan) + 4;
        
        if (userData.History.JoinDates.Count > userInfractionJoinBuffer)
            sb.AppendLine("Account has joined more than four times excluding removals by infractions.");
        
        if (userData.History.JoinDates.Count(jd => jd.AddDays(14) > DateTimeOffset.UtcNow) > 3)
            sb.AppendLine("Account has joined more than three times in the last two weeks.");


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
        
        return await _channelLogger.LogAsync(config.LoggingConfig.UseWebhookLogging, channel, null, embed);
    }
}