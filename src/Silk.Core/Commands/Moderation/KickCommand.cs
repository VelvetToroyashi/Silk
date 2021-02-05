using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class KickCommand : BaseCommandModule
    {
        private readonly ILogger<KickCommand> _logger;
        private readonly IDatabaseService _dbService;
        private readonly IInfractionService _infractionService;

        public KickCommand(ILogger<KickCommand> logger, IDatabaseService dbService, IInfractionService incractionService) => 
            (_logger, _dbService, _infractionService) = (logger, dbService, incractionService);

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [RequireBotPermissions(Permissions.KickMembers)]
        [Description("Boot someone from the guild!")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            

            if (user.IsAbove(bot) || ctx.User == user)
            {
                DiscordEmbed embed = await this.CreateHierarchyEmbedAsync(ctx, bot, user);
                await ctx.RespondAsync(embed).ConfigureAwait(false);
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.Blurple)
                    .WithThumbnail(ctx.Guild.IconUrl)
                    .WithDescription($"You've been kicked from `{ctx.Guild.Name}`!")
                    .AddField("Reason:", reason);


                User databaseUser = await _dbService.GetOrCreateGuildUserAsync(ctx.Guild.Id, user.Id);
                Infraction infraction = await _infractionService.CreateInfractionAsync(user, ctx.Member, InfractionType.Kick, reason!);
                string messaged;
                try
                {
                    await user.SendMessageAsync(embed).ConfigureAwait(false);
                    messaged = "(User notified with Direct message)";
                }
                catch (InvalidOperationException)
                {
                    _logger.LogWarning("Couldn't DM member when notifying kick.");
                    messaged = "(Could not message user.)";
                }
                
                await _infractionService.VerboseKickAsync(user, ctx.Channel, infraction, new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SpringGreen)
                    .WithDescription($":boot: Kicked {user.Mention}! {messaged}")
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
                _ when isMod => $"I can't kick {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})",
                _ when isAdmin => $"I can't kick {user.Mention}! They're an admin! ({user.Roles.Last().Mention})",
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