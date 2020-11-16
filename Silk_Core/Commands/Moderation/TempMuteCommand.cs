using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SilkBot.Extensions;
using SilkBot.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;
using SilkBot.Utilities;

namespace SilkBot.Commands.Moderation.Temporary_Moderation
{
    [Category(Categories.Mod)]
    public class TempMuteCommand : BaseCommandModule
    {
        public IDbContextFactory<SilkDbContext> DbFactory { private get; set; }
        public TimedEventService EventService { private get; set; }
        public DiscordClient Client { private get; set; }

        [Command("Mute")]
        public async Task TempMute(CommandContext ctx, DiscordMember user, string duration,
            [RemainingText] string reason = "Not Given.")
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable()
                .First(g => g.Id == ctx.Guild.Id);
            if (!bot.HasPermission(Permissions.ManageRoles))
            {
                var message = await ctx.RespondAsync("Sorry, but I don't have permission to mute members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }

            if (!ctx.Member.HasPermission(Permissions.ManageRoles))
            {
                var message = await ctx.RespondAsync("Sorry, but you don't have permission to mute members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }

            if (config.MuteRoleId == default)
            {
                await ctx.RespondAsync("Muted role is not set up");
                return;
            }


            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync("User is above bot");
                return;
            }

            var _duration = GetTimeFromInput(duration);
            if (_duration.Duration() == TimeSpan.Zero)
            {
                var msg = await ctx.RespondAsync("Couldn't determine time from message!");
                await Task.Delay(10000);
                await msg.DeleteAsync();
            }

            var tempMute = new TimedInfraction(user.Id, ctx.Guild.Id, DateTime.Now.Add(_duration), reason, (e) => OnMuteExpired((TimedInfraction)e));

            EventService.Events.Add(tempMute);

            await user.GrantRoleAsync(ctx.Guild.GetRole(config.MuteRoleId), reason);
        }


        private async void OnMuteExpired(TimedInfraction eventObject)
        {

            var unmuteMember = await (await Client.GetGuildAsync(eventObject.Guild)).GetMemberAsync(eventObject.Id);

            var guild = DbFactory.CreateDbContext().Guilds
                .First(g => g.Id == eventObject.Guild);
            ulong muteRole = guild.MuteRoleId;

            await unmuteMember.RevokeRoleAsync((await Client.GetGuildAsync(eventObject.Guild)).Roles[muteRole]);
        }

        private TimeSpan GetTimeFromInput(string input) =>
            input.ToLower()[^1] switch
            {
                'm' => TimeSpan.FromMinutes(double.Parse(input[0..^1])),
                'h' => TimeSpan.FromHours(double.Parse(input[0..^1])),
                'd' => TimeSpan.FromDays(double.Parse(input[0..^1])),
                'w' => TimeSpan.FromDays(7 * double.Parse(input[0..^1])),
                _ => TimeSpan.Zero
            };
    }
}