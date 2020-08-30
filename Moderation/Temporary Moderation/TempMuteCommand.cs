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
    public sealed class TempMuteCommand : BaseCommandModule
    {
        [Command("Mute")]
        public async Task TempMute(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = "Not Given.")
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
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
            if(SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.MutedRole == default)
            {
                await ctx.RespondAsync("Muted role is not set up");
                return;
            }

            var (Guild, _) = SilkBot.Bot.Instance.Data[ctx.Guild];
            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync("User is above bot");
                return;
            }
            try
            {
                var _duration = GetTimeFromInput(duration);
                var tempMute = new TimedRestrictionAction()
                {
                    ActionReason = RestrictionActionReason.TemporaryMute,
                    Expiration = DateTime.Now.Add(_duration),
                    Guild = ctx.Guild,
                    Id = user.Id,
                    Reason = reason
                };
                await user.GrantRoleAsync(ctx.Guild.Roles[Guild.MutedRole]);


            }
            catch (InvalidOperationException)
            {
                var msg = await ctx.RespondAsync("Couldn't determine time from message!");
                await Task.Delay(10000);
                await msg.DeleteAsync();
            }
            

        }

        public TempMuteCommand() => SilkBot.Bot.Instance.Timer.Unmute += OnMuteExpired;

        private async void OnMuteExpired(object sender, EventArgs e)
        {
            if (sender is null) return;
            var actionObject = sender as TimedRestrictionAction;
            var unmuteMember = await actionObject.Guild.GetMemberAsync(actionObject.Id);
            var muteRole = SilkBot.Bot.Instance.Data[actionObject.Guild].GuildInfo.MutedRole;
            await unmuteMember.RevokeRoleAsync(actionObject.Guild.Roles[muteRole]);
        }

        private TimeSpan GetTimeFromInput(string input) =>
            input.ToLower()[^1] switch
            {
                'm' => TimeSpan.FromMinutes(double.Parse(input[0..^1])),
                'h' => TimeSpan.FromHours(double.Parse(input[0..^1])),
                'd' => TimeSpan.FromDays(double.Parse(input[0..^1])),
                'w' => TimeSpan.FromDays(7 * double.Parse(input[0..^1])),
                 _  => throw new InvalidOperationException()
            };
    }
}
