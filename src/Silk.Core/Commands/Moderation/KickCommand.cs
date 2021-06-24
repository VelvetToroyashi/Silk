using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class KickCommand : BaseCommandModule
    {
        private readonly IModerationService _moderationService;
        private readonly ILogger<KickCommand> _logger;
        private readonly IInfractionService _infractionService;
        public KickCommand(ILogger<KickCommand> logger, IModerationService moderationService, IInfractionService infractionService)
        {
            _logger = logger;
            _moderationService = moderationService;
            _infractionService = infractionService;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [RequirePermissions(Permissions.KickMembers)]
        [Description("Boot someone from the guild!")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            if (!user.IsAbove(bot) && !user.IsAbove(ctx.Member) && ctx.User != user)
            {
                var response = await _infractionService.KickAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);
                var message = response switch
                {
                    InfractionResult.FailedGuildHeirarchy => "I can't kick that person due to role heiarchy!",
                    InfractionResult.FailedSelfPermissions => "I don't have permission to kick members!", /* In rectrospect, these should never happen, but. */
                    InfractionResult.SucceededWithNotification => $"Kicked {Formatter.Bold($"{user.Username}#{user.Discriminator}")}  (Notified with direct message).",
                    InfractionResult.SucceededWithoutNotification => $"Kicked {Formatter.Bold($"{user.Username}#{user.Discriminator}")} (Unable to notify with Direct Message)."
                };

                await ctx.RespondAsync(message);
            }
            else
            {
                DiscordEmbed embed = await CreateHierarchyEmbedAsync(ctx, bot, user);
                await ctx.RespondAsync(embed);
            }
        }

        private static async Task<DiscordEmbedBuilder> CreateHierarchyEmbedAsync(CommandContext ctx, DiscordMember bot, DiscordMember user)
        {
            bool isBot = user == bot;
            bool isOwner = user == ctx.Guild.Owner;
            bool isMod = user.HasPermission(Permissions.KickMembers);
            bool isAdmin = user.HasPermission(Permissions.Administrator);
            bool isCurrent = ctx.User == user;
            string errorReason = user.IsAbove(bot) switch
            {
                _ when isBot => "I wish I could kick myself, but I sadly cannot.",
                _ when isOwner => $"I can't kick the owner ({user.Mention}) out of their own server!",
                _ when isMod => $"I can't kick {user.Mention}! They're a staff! ({user.Roles.Last().Mention})",
                _ when isAdmin => $"I can't kick {user.Mention}! They're staff! ({user.Roles.Last().Mention})",
                _ when isCurrent => "Very funny, I like you, but no, you can't kick yourself.",
                _ => "Something has gone really wrong, and I don't know what *:(*"
            };

            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                .WithDescription(errorReason)
                .WithColor(DiscordColor.Red);
        }
    }
}