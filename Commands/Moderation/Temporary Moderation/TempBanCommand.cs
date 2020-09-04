using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Tools;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.Temporary_Moderation
{
    public class TempBanCommand : BaseCommandModule
    {
        [Command("tempban")]
        public async Task TempBan(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = null)
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!bot.HasPermission(Permissions.BanMembers))
            {
                var message = await ctx.RespondAsync("Sorry, but I don't have permission to ban members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }
            if (!ctx.Member.HasPermission(Permissions.BanMembers))
            {
                var message = await ctx.RespondAsync("Sorry, but you don't have permission to ban members!");
                await Task.Delay(10000);
                await message.DeleteAsync();
                return;
            }
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
          
            if (!config.DiscordUserInfos.FirstOrDefault(m => m.UserId == ctx.User.Id).Flags.HasFlag(Models.UserFlag.Staff))
            {
                await ctx.RespondAsync("Only staff members can use this command");
                return;
            }
            else if (user.IsAbove(bot))
            {
                await ctx.RespondAsync("User is above bot");
            }
            
            await ctx.Guild.BanMemberAsync(user.Id, reason: reason);
            
            try 
            {
                var _duration = GetTimeFromInput(duration);
                var tempBan = new TimedRestrictionAction()
                {
                    ActionReason = RestrictionActionReason.TemporaryBan,
                    Expiration = DateTime.Now.Add(_duration),
                    Guild = ctx.Guild,
                    Id = user.Id,
                    Reason = reason
                };


            }
            catch (InvalidOperationException) 
            {
                var msg = await ctx.RespondAsync("Couldn't determine time from message!");
                await Task.Delay(10000);
                await msg.DeleteAsync();
            }


        }

        private TimeSpan GetTimeFromInput(string input) =>
            input.ToLower()[^1] switch
            {
                'm' => TimeSpan.FromMinutes(double.Parse(input[0..^1])),
                'h' => TimeSpan.FromHours(double.Parse(input[0..^1])),
                'd' => TimeSpan.FromDays(double.Parse(input[0..^1])),
                'w' => TimeSpan.FromDays(7 * double.Parse(input[0..^1])),
                _   => throw new InvalidOperationException()
            };
        

        public TempBanCommand()
        {
            SilkBot.Bot.Instance.Timer.UnBan += OnBanExpiration;
        }

        private void OnBanExpiration(object sender, EventArgs e)
        {
            if (sender is null) return;
            var actionObject = sender as TimedRestrictionAction;
            actionObject.Guild.UnbanMemberAsync(actionObject.Id, "Temporary ban completed.");
        }
    }
}
