using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Discord.Commands.Moderation.Ban
{
    [Category(Categories.Mod)]
    public class BanCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public BanCommand(IDbContextFactory<GuildContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command("ban")]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Ban a member from the Guild")]
        public async Task Ban(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason given.")
        {
            DiscordMember user = await ctx.Guild.GetMemberAsync(target.Id);
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!CanExecuteCommand(out string errorReason))
            {
                await DenyBanAsync(errorReason);
                return;
            }

            async Task DenyBanAsync(string errorReason)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.Red)
                    .WithDescription(errorReason));
            }

            bool CanExecuteCommand(out string errorReason)
            {
                if (target == bot)
                {
                    errorReason = "I can't ban myself!";
                    return false;
                }
                if (!ctx.Member.HasPermission(Permissions.BanMembers))
                {
                    errorReason = "You do not have permission to ban members!";
                    return false;
                }
                if (user.IsAbove(bot))
                {
                    errorReason = $"{target.Mention} has a role {user.GetRoleMention()} that is above mine, and I cannot ban them!";
                    return false;
                }

                errorReason = null!;
                return true;
            }

            DiscordEmbedBuilder userBannedEmbed = new DiscordEmbedBuilder()
                .WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl)
                .WithTitle($"You've been banned from {ctx.Guild.Name}!")
                .AddField("Reason:", $"{reason}")
                .AddFooter(ctx)
                .WithColor(new DiscordColor("#cc1400"));

            (string name, string url) = ctx.GetAuthor();
            DiscordEmbedBuilder logEmbed = new DiscordEmbedBuilder()
                .WithAuthorExtension(name, url)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($":hammer: {ctx.Member.Mention} banned {target.Mention}!")
                .AddField("Infraction occured:", DateTime.UtcNow.ToString("dd/MM/yy - HH:mm UTC"))
                .AddField("Reason:", reason)
                .AddFooter(ctx);
            try
            {
                await target.SendMessageAsync(userBannedEmbed);
            }
            finally
            {
                await ctx.Guild.BanMemberAsync(user, 7, reason);
                ulong? loggingChannel = _dbFactory.CreateDbContext()
                    .Guilds.FirstOrDefault(g => g.Id == ctx.Guild.Id)
                    ?.Configuration.LoggingChannel;
                DiscordChannel sendChannel = ctx.Guild.GetChannel(loggingChannel!.Value) ?? ctx.Channel;

                await sendChannel.SendMessageAsync(logEmbed);
            }
        }
    }
}