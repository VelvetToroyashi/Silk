using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Logic.Commands.Moderation.Ban
{
    [Category(Categories.Mod)]
    public class TempBanCommand : BaseCommandModule
    {
        private readonly IInfractionService _infractionService;
        public TempBanCommand(IInfractionService infractionService)
        {
            _infractionService = infractionService;
        }

        [RequireGuild]
        [Command("tempban")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Temporarily ban a member from the Guild")]
        public async Task TempBan(CommandContext ctx, DiscordUser user, TimeSpan duration, [RemainingText] string reason = "Not provided.")
        {
            DiscordMember member;

            try
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync("Member not found on server!");
                return;
            }

            if (DateTime.Now + duration > DateTime.Now + TimeSpan.FromDays(365))
            {
                await ctx.RespondAsync("Can't temp-ban member for more than 1 year!");
                return;
            }


            try { await member.BanAsync(0, reason); }
            catch (NotFoundException)
            {
                await ctx.RespondAsync("User isn't on the server!");
                return;
            }

            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.SpringGreen)
                .WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl)
                .WithTitle($"{user.Username} was temporarily banned from {ctx.Guild.Name}!")
                .AddField("Duration:", duration.Humanize(1, CultureInfo.CurrentCulture, TimeUnit.Year, TimeUnit.Second), true)
                .AddField("Enforcer:", $"{ctx.User.Mention} ({ctx.User.Id})", true)
                .AddField("Reason:", reason);

            DateTime dur = DateTime.Now + duration;
            Infraction infraction = await _infractionService.CreateTempInfractionAsync(member, ctx.Member, InfractionType.SoftBan, reason, dur);

            await _infractionService.TempBanAsync(member, ctx.Channel, infraction, embed);
        }
    }
}