using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SilkBot.Database;
using SilkBot.Extensions;
using SilkBot.Models;

namespace SilkBot.Commands.Bot
{
    public static class BotEvents
    {
        public static async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            
            

        }

        // Set when !clear x is called, as to prevent logging messages cleared by the bot. //
        public static int UnloggedMessages { get; set; } 


        public static async Task OnMessageDeleted(DiscordClient c, MessageDeleteEventArgs e)
        {
            

            if (UnloggedMessages - 1 > 0)
            {
                UnloggedMessages--;
                e.Handled = true;
                return;
            }

            if (e.Channel.IsPrivate || e.Message.Author.IsCurrent) return;

            try
            {
                
                GuildModel config = SilkBot.Bot.Instance!.SilkDBContext.Guilds.First(g => g.Id == e.Guild.Id); 
                
                if (!config.LogMessageChanges || config.MessageEditChannel == default) return;
                
                DiscordEmbedBuilder embed =
                    new DiscordEmbedBuilder()
                        .WithTitle("Message Deleted:")
                        .WithDescription(
                            $"User: {e.Message.Author.Mention}\n" +
                            $"Channel: {e.Channel.Mention}\n" +
                            $"Time: {DateTime.Now:HH:mm}\n" +
                            $"Message Contents: ```\n{e.Message.Content}```")
                        .AddField("Message ID:", e.Message.Id.ToString(), true)
                        .AddField("User ID:", e.Message.Author.Id.ToString(), true)
                        .WithThumbnail(e.Message.Author.AvatarUrl)
                        .WithColor(DiscordColor.Red)
                        .WithFooter("Silk!", c.CurrentUser.AvatarUrl)
                        .WithTimestamp(DateTime.Now);
                DiscordChannel loggingChannel = await c.GetChannelAsync(config.MessageEditChannel);
                await c.SendMessageAsync(loggingChannel, embed: embed);
            }
            catch(Exception ex) {Log.Logger.Warning($"{ex}");}
            

            



        }
    }
}