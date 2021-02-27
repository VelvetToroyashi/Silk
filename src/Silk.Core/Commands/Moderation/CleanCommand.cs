using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Silk.Core.Utilities;
using Silk.Data.Models;

namespace Silk.Core.Commands.Moderation
{
    public class CleanCommand : BaseCommandModule
    {
        private readonly RootCommand _command;

        public CleanCommand()
        {
            _command = new();
            _command.AddOption(new("-i") { IsRequired = false });             // Images (png, jpg, and jpeg)
            _command.AddOption(new("-b") { IsRequired = false });             // Bots
            _command.AddOption(new Option<ulong>("-u")  { IsRequired = false });    // User
            _command.AddOption(new Option<ulong>("-c")  { IsRequired = false });    // Channel
            _command.AddOption(new Option<string>("-r") { IsRequired = false });    // Regex that mf
            
        }
        
        [Command]
        [RequireFlag(UserFlag.Staff)]
        [Description(
                     "Clean message! options is...optional, and options are as follows:\n" +
                     "-i -- Cleans Images\n" +
                     "-b -- Cleans bot messages\n" +
                     "-u <Id> -- Cleans messages from the specified user\n" +
                     "-c <Id> -- Cleans messages in a different channel\n" +
                     "-r <Text> -- Cleans messages that match regex\n" +
                     "\nRemarks: -u will override -b")]
        
        public async Task Clean(CommandContext ctx, int messages, [RemainingText] string? options)
        {
            _command.Handler = CommandHandler
                .Create<bool, bool, ulong, ulong, string>(async (i, b, u, c, r) => 
                    await GetResult(ctx, messages, i, b, u, c, r));

            try
            {
                await _command.InvokeAsync(options?.Split(' ') ?? new string[] { });
            }
            catch
            {
                // ignored
            } //CBA to deal with System.CommandLine exceptions
        }

        private async Task GetResult(CommandContext ctx,int messageCount, bool images, bool bots, ulong user, ulong channel, string regex)
        {
            DiscordMessage? cleaningMessage = await ctx.RespondAsync(":warning: Cleaning...");
            ulong chnId = channel is 0 ? ctx.Channel.Id : channel;

            try
            {
                DiscordChannel? chn = ctx.Guild.Channels[chnId];

                IEnumerable<DiscordMessage>? apiMessages = (await chn.GetMessagesAsync()).AsEnumerable();
                IEnumerable<DiscordMessage> messageList = new List<DiscordMessage>();
                
                // Attempt to get the requisite amount of messages 5 times, and then delete whatever we have anyway. //
                int tries = 0;
                while (tries < 5)
                {
                    IEnumerable<DiscordMessage> tempMessages;
                    if (!string.IsNullOrEmpty(regex))
                    {
                        tempMessages = apiMessages.Where(m => Regex.IsMatch(m.Content, regex)).Take(10);
                        messageList = messageList.Union(tempMessages);
                    }
                    else if (user is not 0)
                    {
                        apiMessages = apiMessages.Where(m => m.Author.Id == user);
                    }
                    else if (bots && user is 0)
                    {
                        tempMessages = apiMessages.Except(new [] { cleaningMessage }).Where(m => m.Author.IsBot).Take(messageCount);
                        messageList = messageList.Union(tempMessages);
                    }
                    else
                    {
                        if (images)
                        {
                            tempMessages = apiMessages
                                .Where(m =>
                                    m.Attachments.Any(a =>
                                        a.FileName.EndsWith(".png") ||
                                        a.FileName.EndsWith(".jpg") ||
                                        a.FileName.EndsWith(".jpeg")) ||
                                    m.Embeds.Any(e => e.Type is "image"))
                                .Take(messageCount);
                            messageList = messageList.Union(tempMessages);
                        }
                        else
                        {
                            messageList = apiMessages.Take(messageCount);
                        }
                    }
                    
                    if (messageList.Count() < messageCount)
                    {
                        apiMessages = await chn.GetMessagesBeforeAsync(apiMessages.Last().Id);
                        tries++;
                    }
                    else break;
                }
                if (messageList.Count() is not 0)
                    await chn.DeleteMessagesAsync(messageList);
            }
            catch (KeyNotFoundException)
            {
                await ctx.RespondAsync("**`-c: Invalid channel Id!`**");
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync("**`-u: Invalid user Id!`**");
            }
            finally
            {
                await cleaningMessage.DeleteAsync();
            }
        }
    }
}