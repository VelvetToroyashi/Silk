using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class KickCommand : BaseCommandModule
    {
        private readonly ILogger<KickCommand> _logger;
        private readonly IInfractionService _infractionService;

        public KickCommand(ILogger<KickCommand> logger, IInfractionService infractionService)
        {
            _logger = logger;
            _infractionService = infractionService;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [RequireBotPermissions(Permissions.KickMembers)]
        [Description("Boot someone from the guild!")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            if (user.IsAbove(bot) || user.IsAbove(ctx.Member) || ctx.User == user)
            {
                DiscordEmbed embed = await CreateHierarchyEmbedAsync(ctx, bot, user);
                await ctx.RespondAsync(embed);
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.Blurple)
                    .WithThumbnail(ctx.Guild.IconUrl)
                    .WithDescription($"You've been kicked from `{ctx.Guild.Name}`!")
                    .AddField("Reason:", reason);

                Infraction infraction = await _infractionService.CreateInfractionAsync(user, ctx.Member, InfractionType.Kick, reason!);
                string message = string.Empty;
                try
                {
                    await user.SendMessageAsync(embed);
                    message = "(User notified with Direct Message).";
                }
                catch (ArgumentException) { }
                catch (UnauthorizedException)
                {
                    _logger.LogWarning("Couldn't DM member when notifying kick");
                    message = "(Could not message user).";
                }

                await _infractionService.KickAsync(user, ctx.Channel, infraction, new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SpringGreen)
                    .WithDescription($":boot: Kicked {user.Mention}! {message}")
                    .AddField("Reason:", reason)
                    .AddField("Time:", DateTime.UtcNow.ToString("MM/dd/yyyy - h:mm UTC")));
            }
        }

        private async Task<DiscordEmbedBuilder> CreateHierarchyEmbedAsync(CommandContext ctx, DiscordMember bot, DiscordMember user)
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