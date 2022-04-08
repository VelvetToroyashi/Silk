using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Guild;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Moderation;

[RequireDiscordPermission(DiscordPermission.ManageChannels)]
[RequireBotDiscordPermissions(DiscordPermission.ManageChannels)]
public class ChannelLockCommands : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly ChannelLockerService   _locker;
    private readonly IDiscordRestChannelAPI _channels;
    
    public ChannelLockCommands(ICommandContext context, ChannelLockerService locker, IDiscordRestChannelAPI channels)
    {
        _context  = context;
        _locker   = locker;
        _channels = channels;
    }
    
    [Command("lock")]
    [Description("Locks a channel either temporarily or indefinitely.")]
    public async Task<IResult> LockAsync
    (
        [Description("The channel to lock. (Default: current channel)")]
        //[RequireBotDiscordPermissions(DiscordPermission.ManageChannels)]
        IChannel? channel = null,
        
        [Option('r', "reason")]
        [Description("Why the channel is being locked")]
        string? reason = null,
        
        [Option('f', "for")]
        [Description("How long to lock the channel for. Defaults to indefinitely.")]
        string? duration = null
    )
    {
        var channelID = channel?.ID ?? _context.ChannelID;
        
        var temporary = duration is not null;

        var parsedDuration = TimeHelper.Extract(duration);

        if (temporary && parsedDuration is null)
            return await _channels.CreateMessageAsync(_context.ChannelID, "Sorry, but I can't tell how long you wanted to lock the channel for.");

        var lockResult = await _locker.LockChannelAsync(_context.User.ID, channelID, _context.GuildID.Value, parsedDuration, reason ?? "Not specified");
        
        if (!lockResult.IsSuccess)
            return await _channels.CreateMessageAsync(_context.ChannelID, "Something went wrong while locking the channel, sorry!");

        string message = channel is null 
            ? $"{Emojis.MuteEmoji} This channel has been locked! Automatically unlocking {(temporary ? $"{parsedDuration.Value.ToTimestamp()}" : "indefinitely")}." 
            : $"{Emojis.MuteEmoji} Locked {channel.Mention()}! Automatically unlocking {(temporary ? $"{parsedDuration.Value.ToTimestamp()}" : "indefinitely")}.";
        
        return await _channels.CreateMessageAsync(_context.ChannelID, message);
    }
}