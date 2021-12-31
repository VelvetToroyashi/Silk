using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Silk.Commands.General;

public class AvatarSlashCommands //: CommandGroup
{
    private readonly ICommandContext            _context;
    private readonly IDiscordRestGuildAPI       _guilds;
    private readonly IDiscordRestInteractionAPI _interactions;
    public AvatarSlashCommands(
        ICommandContext context,
        IDiscordRestGuildAPI guilds,
        IDiscordRestInteractionAPI interactions
        )
    {
        _context      = context;
        _guilds       = guilds;
        _interactions = interactions;
    }

    
    [Ephemeral]
    [Command("avatar")]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Get the avatar of a user, including yourself!")]
    public async Task<Result> Avatar
        (
            [Description("The user to get the avatar of.")]
            IUser? user = null,
            [Description("Whether to get the user's guild avatar.")]
            bool guild = false
        )
    {
        if (_context is not InteractionContext ic)
            return Result.FromSuccess();

        user ??= ic.User;
        
        IEmbed embed;

        if (!guild)
        {
            var avatarURLResult = user.Avatar is null ? CDN.GetDefaultUserAvatarUrl(user, imageSize: 4096) : CDN.GetUserAvatarUrl(user, imageSize: 4096);

            if (!avatarURLResult.IsSuccess)
                return Result.FromError(avatarURLResult.Error);

            embed = new Embed
            {
                Colour = Color.DodgerBlue,
                Title  = $"{user.Username}'s Avatar",
                Image  = new EmbedImage(avatarURLResult.Entity.ToString())
            };
        }
        else
        {
            if (!ic.GuildID.IsDefined(out var guildID))
                return Result.FromError(new InvalidOperationError("Getting guild avatars requires being on a guild!"));

            var member = await _guilds.GetGuildMemberAsync(guildID, user.ID);

            if (!member.IsSuccess)
                return Result.FromError(new NotFoundError("It appears the user is not in this guild!"));

            if (!member.Entity.Avatar.IsDefined())
                return Result.FromError(new NotFoundError("It appears the user has no guild avatar!"));

            var avatarURLResult = CDN.GetGuildMemberAvatarUrl(guildID, member.Entity, imageSize: 4096);

            if (!avatarURLResult.IsSuccess)
                return Result.FromError(avatarURLResult.Error);

            embed = new Embed
            {
                Colour = Color.DodgerBlue,
                Title  = $"{user.Username}'s Guild Avatar",
                Image  = new EmbedImage(avatarURLResult.Entity.ToString())
            };
        }
        
        await _interactions.EditOriginalInteractionResponseAsync(ic.ApplicationID, ic.Token, embeds: new[] { embed });
        
        return Result.FromSuccess();
    }
}