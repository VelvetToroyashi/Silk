using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {
        private readonly Regex EmojiRegex = new(@"<a?:(.+):([0-9]+)>");

        private IMediator _mediator;
        public RoleMenuCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [Description("Automagically configure a role menu based on a message! Must provide message link!")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink)
        {
            var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (config.ReactionRoles.Any(r => r.MessageId == messageLink.Id))
            {
                await ctx.RespondAsync($"That role menu is already set up! use `{ctx.Prefix}rolemenu fix` to fix/update it!");
                return;
            }


            var roles = messageLink.MentionedRoles;
            var waitMessage = await ctx.RespondAsync("Got it! This may take a few seconds.");
            var loading = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.LoadingId);
            var failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);
            DiscordMessage progress = await waitMessage.RespondAsync(loading.ToString());

            MatchCollection matches = EmojiRegex.Matches(messageLink.Content);

            if (matches.Count is 0)
            {
                await progress.DeleteAsync();
                await waitMessage.ModifyAsync("There don't seem to be any emojis in that message!");
                return;
            }

            await progress.ModifyAsync($"{loading} Found {matches.Count} emojis...");
            await Task.Delay(400);

            // Ensure every role has a matching emoji. //
            if (messageLink.MentionedRoles.Count > matches.Count)
            {
                await progress.ModifyAsync($"{failed} : Mismatched emoji and role count!");
                return;
            }

            var emojis = new ulong[matches.Count];

            for (int i = 0; i < messageLink.MentionedRoles.Count; i++)
            {

                await Task.Delay(700);
            }
        }
    }
}