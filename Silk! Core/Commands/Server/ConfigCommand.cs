using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Server
{
    public class ConfigCommand : BaseCommandModule
    {

        //TODO: Fix configure command to allow for interactive or static via subcomands
        [Command("configure"), RequireFlag(UserFlag.Staff)]
        public async Task GuildConfigurationCommand(CommandContext ctx)
        {
            using var db = new SilkDbContext();
            var guild = db.Guilds.First(g => g.DiscordGuildId == ctx.Guild.Id);
            var sentConfigurationMessage = await ctx.RespondAsync("1: Toggle whitelisting invites, 2: Log message edits and deletions, 3: Log member joins and leaves, 4: Log role changes, 5: Set mute role, 6: Set log channel");
            var interactivity = ctx.Client.GetInteractivity();

            var configMessage = await interactivity.WaitForMessageAsync(message => message.Author == ctx.Member && Regex.IsMatch(message.Content, "[1-6]"), TimeSpan.FromSeconds(30));
            


            if (configMessage.TimedOut)
            {
                await sentConfigurationMessage.DeleteAsync();
                await ctx.RespondAsync("Timed out.");
                return;
            }
            switch (int.Parse(configMessage.Result.Content))
            {
                case 1:
                    await WhiteListInvites(guild);
                    break;
                case 2:
                    await ToggleLogMessageChanges(db, guild, ctx.Channel, ctx.User);
                    break;
                //case 751968150660055150:
                //    LogMemberJoinLeave(configurationMessage);
                //    break;
                //case 751968169710715030:
                //    LogRoleChanges(configurationMessage);
                //    break;
                //case 751968193555333251:
                //    SetMute(configurationMessage);
                //    break;
                //case 751968212530364456:
                //    SetGeneralLogChannel(configurationMessage);
                //    break;
                default: break;
            }
        }

        private async Task WhiteListInvites(Models.GuildModel guild)
        {
            guild.WhitelistInvites = !guild.WhitelistInvites;
        }

        private async Task ToggleLogMessageChanges(SilkDbContext db, Models.GuildModel guild,  DiscordChannel channel, DiscordUser user)
        {
            if (guild.LogMessageChanges)
            {
                var eb = new DiscordEmbedBuilder()
                    .WithAuthor(user.Username, null, user.AvatarUrl)
                    .WithDescription($"You've already set up message logging! You can view deleted and edited messages in <#{guild.GeneralLoggingChannel}>!\nDo you want to disable message logging?")
                    .WithColor(DiscordColor.Gold)
                    .WithFooter($"Silk! | Requested: {user.Id}")
                    .WithTimestamp(DateTime.Now);
                var msg = await channel.SendMessageAsync(embed: eb);
                var interactivity = Instance.Client.GetInteractivity();
                var (yes, no) = (DiscordEmoji.FromName(Instance.Client, ":white_check_mark:"), DiscordEmoji.FromName(Instance.Client, ":x:"));
                await msg.CreateReactionAsync(yes);
                await msg.CreateReactionAsync(no);
                
                var reactionResult = await msg.WaitForReactionAsync(user, TimeSpan.FromSeconds(30));
                if (reactionResult.TimedOut)
                {
                    await msg.DeleteAsync("Timed out.");
                    return;
                }
                if (reactionResult.Result.Emoji == yes) 
                { 
                    guild.LogMessageChanges = false;
                    await msg.DeleteReactionAsync(yes, user);
                    await db.SaveChangesAsync(); 
                }
                else if (reactionResult.Result.Emoji == no) { return; /* Do something useful here as well */ }
                else return; //If the user didn't react with proper input, that's on them.

            }
            else await SetupLogging(channel, user, db);

            

            async Task SetupLogging(DiscordChannel c, DiscordUser u, SilkDbContext d) 
            {
                await c.SendMessageAsync("Great. What channel would you like to log to?");
                var logChannel = await Instance.Client.GetInteractivity().WaitForMessageAsync(m => m.Author == u && m.MentionedChannels.Count > 0);
                var guild = db.Guilds.AsQueryable().First(g => g.DiscordGuildId == c.GuildId);
                guild.GeneralLoggingChannel = logChannel.Result.MentionedChannels[0].Id;
                guild.LogMessageChanges = true;
                await db.SaveChangesAsync();
                await c.SendMessageAsync($"Great! I'll log to <#{guild.GeneralLoggingChannel}>");
            }
        }

        //private async Task LogMemberJoinLeave(DiscordMessage configurationMessage)
        //{
        //    await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968150660055150), configurationMessage.Author);
        //}

        //private async Task LogRoleChanges(DiscordMessage configurationMessage)
        //{
        //    await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968169710715030), configurationMessage.Author);
        //}

        //private async Task SetMute(DiscordMessage configurationMessage)
        //{
        //    await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968193555333251), configurationMessage.Author);
        //}

        //private async Task SetGeneralLogChannel(DiscordMessage configurationMessage)
        //{
        //    await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968212530364456), configurationMessage.Author);
        //}


    }
}
