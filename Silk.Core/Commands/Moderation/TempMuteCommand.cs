using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Tools;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class TempMuteCommand : BaseCommandModule
    {
        public IDbContextFactory<SilkDbContext> DbFactory { private get; set; }
        public TimedEventService EventService { private get; set; }
        public DiscordClient Client { private get; set; }

        [Command("Mute")]
        public async Task TempMute(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            GuildModel config = Core.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.Id == ctx.Guild.Id);
            
            if (!bot.HasPermission(Permissions.ManageRoles))
            {
                DiscordMessage message = await ctx.RespondAsync("Sorry, but I don't have permission to mute members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }

            if (!ctx.Member.HasPermission(Permissions.ManageRoles))
            {
                DiscordMessage message = await ctx.RespondAsync("Sorry, but you don't have permission to mute members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }

            if (config.Configuration.MuteRoleId == default)
            {
                await ctx.RespondAsync("Muted role is not set up");
                return;
            }

            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync("User is above bot");
                return;
            }

            TimeSpan _duration = GetTimeFromInput(duration);
            if (_duration.Duration() == TimeSpan.Zero)
            {
                DiscordMessage msg = await ctx.RespondAsync("Couldn't determine time from message!");
                await Task.Delay(10000);
                await msg.DeleteAsync();
            }

            var tempMute = new TimedInfraction(user.Id, ctx.Guild.Id, DateTime.Now.Add(_duration), reason, 
                e => OnMuteExpired((TimedInfraction) e));

            EventService.Events.Add(tempMute);

            await user.GrantRoleAsync(ctx.Guild.GetRole(config.Configuration.MuteRoleId), reason);
        }


        private async void OnMuteExpired(TimedInfraction eventObject)
        {
            DiscordMember unmuteMember = await (await Client.GetGuildAsync(eventObject.Guild)).GetMemberAsync(eventObject.Id);

            GuildModel guild = DbFactory.CreateDbContext().Guilds.First(g => g.Id == eventObject.Guild);
            
            ulong muteRole = guild.Configuration.MuteRoleId;

            await unmuteMember.RevokeRoleAsync((await Client.GetGuildAsync(eventObject.Guild)).Roles[muteRole]);
        }

        private TimeSpan GetTimeFromInput(string input)
        {
            return input.ToLower()[^1] switch
            {
                'm' => TimeSpan.FromMinutes(double.Parse(input[..^1])),
                'h' => TimeSpan.FromHours(double.Parse(input[..^1])),
                'd' => TimeSpan.FromDays(double.Parse(input[..^1])),
                'w' => TimeSpan.FromDays(7 * double.Parse(input[..^1])),
                _ => TimeSpan.Zero
            };
        }
    }
}