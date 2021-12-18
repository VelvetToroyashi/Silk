using System.ComponentModel;
using System.Drawing;
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

namespace Silk.Commands.General;

public class AvatarSlashCommands : CommandGroup
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

    [Command("avatar_slash")]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Get the avatar of a user, including yourself!")]
    public async Task<Result> Avatar
        (
            [Description("The user to get the avatar of.")]
            IUser? user = null,
            [Description("Whether to get the user's guild avatar.")]
            bool showGuildAvatar = false,
            [Description("Whether to show the user's avatar privately.")]
            bool showPrivately = false
        )
    {
        if (_context is not InteractionContext ic)
            return Result.FromSuccess();

        user ??= ic.User;
        
        var responseResult = await _interactions
           .CreateInteractionResponseAsync(
                                            ic.ID,
                                            ic.Token,
                                            new InteractionResponse(
                                                                    InteractionCallbackType.DeferredChannelMessageWithSource,
                                                                    new InteractionCallbackData(Flags: showPrivately
                                                                                                    ? InteractionCallbackDataFlags.Ephemeral
                                                                                                    : default)
                                                                    )
                                           );

        if (!responseResult.IsSuccess)
            return Result.FromError(responseResult.Error);

        IEmbed embed;

        if (!showGuildAvatar)
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

            if (!member.Entity.Avatar.IsDefined(out var guildAvatar))
                return Result.FromError(new NotFoundError("It appears the user has no guild avatar!"));

            var avatarURLResult = CDN.GetGuildMemberAvatarUrl(guildID, user.ID, guildAvatar, imageSize: 4096);

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