using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.ReactionRoles;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [RequirePermissions(Permissions.ManageRoles)]
    public class RoleMenuCommand : BaseCommandModule
    {
        private record Result<T>(T? Value, bool Succeeded, string? Reason = null)
        {
            public static implicit operator T?(Result<T> r) => r.Value;
            public static implicit operator bool(Result<T> r) => r.Succeeded;
        }

        private record RoleMenuOption(ulong Role, string EmojiName);

        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;

        public RoleMenuCommand(IMediator mediator, IServiceCacheUpdaterService updater)
        {
            _mediator = mediator;
            _updater = updater;
        }

        [Command]
        [Description("Automagically configure a role menu based on a message! Must provide message link!\n **Warning!:** I will go based on the order of reactions! If you are missing any reactions, then I will skip that role!")]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink)
        {
            if (messageLink.Reactions.Count is 0)
            {
                await ctx.RespondAsync("That message doesn't have any reactions!");
                return;
            }

            if (messageLink.MentionedRoles.Count is 0)
            {
                await ctx.RespondAsync("That message doesn't mention any roles!");
                return;
            }

            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (config.RoleMenus.Any(r => r.MessageId == messageLink.Id))
            {
                await ctx.RespondAsync($"That role menu is already set up! use `{ctx.Prefix}rolemenu fix` to fix/update it!");
                return;
            }

            DiscordEmoji failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);
            DiscordEmoji success = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.ConfirmId);

            StringBuilder sb = new();

            DiscordMessage message = await messageLink.Channel.GetMessageAsync(messageLink.Id);
            DiscordMessageBuilder builder = new();
            builder.WithoutMentions();

            builder.WithContent("Got it! This should only take a few seconds.");
            DiscordMessage progressMessage = await builder.SendAsync(ctx.Channel);
            List<RoleMenuOption> options = new();

            for (int i = 0; i < message.Reactions.Count; i++)
            {
                if (i >= message.MentionedRoles.Count) break;

                if (message.MentionedRoles[i].Position >= ctx.Guild.CurrentMember.Roles.Last().Position)
                {
                    sb.AppendLine($"{failed} {message.MentionedRoles[i].Mention} is unavailable due to heiarchy");
                    builder.WithContent(sb.ToString());
                    await progressMessage.ModifyAsync(builder);
                }
                else
                {
                    options.Add(new(message.MentionedRoles[i].Id, message.Reactions[i].Emoji.Name));

                    sb.AppendLine($"{success} {message.Reactions[i].Emoji.Name} → {message.MentionedRoles[i].Mention}");
                    builder.WithContent(sb.ToString());

                    await progressMessage.ModifyAsync(builder);
                }

                await Task.Delay(1000);
            }

            await _mediator.Send(new AddRoleMenuRequest(config.Id, message.Id, options.ToDictionary(o => o!.EmojiName, o => o!.Role)));

            builder.WithContent("You should be set! I'll look for reactions and give people their roles. Thank you for choosing Silk! <3");
            await ctx.RespondAsync(builder);

            _updater.UpdateGuild(ctx.Guild.Id);
        }
    }
}