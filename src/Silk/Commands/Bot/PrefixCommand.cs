using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Interfaces;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions.Remora;
using Silk.Utilities;
using IMessage = Remora.Discord.API.Abstractions.Objects.IMessage;

namespace Silk.Commands.Bot;

[Category(Categories.Bot)]
public class PrefixCommand : CommandGroup
{
    private const int PrefixMaxLength = 5;
    
    private readonly IMediator              _mediator;
    private readonly ICommandContext        _context;
    private readonly IPrefixCacheService    _cache;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;
    
    public PrefixCommand
    (
        IMediator mediator,
        ICommandContext context,
        IPrefixCacheService cache,
        IDiscordRestGuildAPI guilds,
        IDiscordRestChannelAPI channels
    )
    {
        _mediator = mediator;
        _context  = context;
        _cache    = cache;
        _guilds   = guilds;
        _channels = channels;
    }



    [Command("prefix")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Gets or sets the current prefix for the guild.")]
    public async Task<Result<IMessage>> SetPrefix(string prefix = "")
    {
        var member = await _guilds.GetGuildMemberAsync(_context.GetGuildID(), _context.GetUserID(), this.CancellationToken);

        if (!member.IsSuccess)
            return Result<IMessage>.FromError(member.Error);

        var permissionResult = await member.Entity.HasPermissionAsync(_guilds, _context.GetGuildID(), DiscordPermission.ManageChannels);
        
        if (!permissionResult.IsSuccess)
            return Result<IMessage>.FromError(permissionResult.Error);

        var gprefix = await _cache.RetrievePrefixAsync(_context.GetGuildID());
        
        if (!permissionResult.Entity || string.IsNullOrEmpty(prefix))
            return await _channels.CreateMessageAsync(_context.GetChannelID(), $"I respond to {gprefix}, `/commands` and when you ping me!");
        
        var prefixCheckResult = IsValidPrefix(prefix);

        if (!prefixCheckResult.IsSuccess)
            return await _channels.CreateMessageAsync(_context.GetChannelID(), prefixCheckResult.Error.Message);

        await _mediator.Send(new UpdateGuild.Request(_context.GetGuildID(), prefix));
        
        _cache.UpdatePrefix(_context.GetGuildID(), prefix);
        
        return await _channels.CreateMessageAsync(_context.GetChannelID(), $"Done! I'll respond to `{prefix}` from now on.");
    }

    private Result IsValidPrefix(string prefix)
    {
        if (prefix.Length > PrefixMaxLength)
            return Result.FromError(new ArgumentOutOfRangeError( nameof(prefix), $"Prefix cannot be more than {PrefixMaxLength} characters!"));

        if (!Regex.IsMatch(prefix, "[A-Z!@#$%^&*<>?.]{1,5}", RegexOptions.IgnoreCase))
            return Result.FromError(new ArgumentInvalidError(nameof(prefix),  "Prefix contained an invalid symbol. Valid symbols include ! @ # $ % ^ & * < > ? and A-z (Case insensitive)"));

        return Result.FromSuccess();
    }
}