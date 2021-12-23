using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.General;

[HelpCategory(Categories.General)]
public class AvatarCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;
    
    public AvatarCommand
    (
        ICommandContext        context,
        IDiscordRestGuildAPI   guilds,
        IDiscordRestChannelAPI channels
    )
    {
        _context  = context;
        _guilds   = guilds;
        _channels = channels;
    }
    
    [Command("avatar", "av")]
    [Description("Show your, or someone else's avatar!")]
    public async Task<Result<IMessage>> GetAvatarAsync(
        [Description("The user to get the avatar of. This can be left blank if you wish to get your own avatar.")]
        IUser? user = null, 
        [Switch("guild")] 
        [Description("Get the guild avatar instead of the global avatar.")]
        bool guild = false)
    {
        user ??= _context.User;
        
        if (!guild)
        {
            var avatarURL = CDN.GetUserAvatarUrl(user, imageSize: 4096);
            
            if (!avatarURL.IsSuccess)
                avatarURL = CDN.GetDefaultUserAvatarUrl(user, imageSize: 4096);
            
            if (!avatarURL.IsSuccess)
                return Result<IMessage>.FromError(avatarURL.Error);

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: GetEmbeds(user, avatarURL.Entity, user.ID == _context.User.ID, guild));
        }
        else
        {
            if (!_context.GuildID.IsDefined(out var guildID))
            {
                var returnResult = await _channels.CreateMessageAsync(_context.ChannelID, "You must be in a server to get someone's guild avatar!");
                return returnResult;
            }

            var memberResult = await _guilds.GetGuildMemberAsync(guildID, user.ID);
            
            if (!memberResult.IsSuccess)
                return await _channels.CreateMessageAsync(_context.ChannelID, "I couldn't find that user in this server!");
            
            var member = memberResult.Entity;

            if (!member.Avatar.IsDefined())
                return await _channels.CreateMessageAsync(_context.ChannelID, "That user doesn't have a guild avatar!");
            
            var avatarURL = CDN.GetGuildMemberAvatarUrl(guildID, member, imageSize: 4096);
            
            if (!avatarURL.IsSuccess)
                return await _channels.CreateMessageAsync(_context.ChannelID, "Something went wrong while getting that user's guild avatar!");
            
            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: GetEmbeds(user, avatarURL.Entity, user is null, guild));
        }
    }
    
    private IEmbed[] GetEmbeds(IUser user, Uri avatar, bool isSelf, bool isGuild)
        => new[]
        {
            new Embed()
            {
                Title = (isSelf ? "Your " : $"{user.Username}'s ") + (isGuild ? "guild" : "") + " avatar!",
                Colour = Color.MidnightBlue,
                Image = new EmbedImage(avatar.ToString())
            }
        };
}