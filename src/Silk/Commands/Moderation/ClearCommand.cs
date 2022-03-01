using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.General;


[HelpCategory(Categories.Mod)]
public class ClearCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    public ClearCommand(ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _context  = context;
        _channels = channels;
    }


    [Command("clear")]
    [Description("Cleans all messages from all users.\n"                   +
                 "It's important to understand the order of precedence:\n" +
                 "Skip ➜ User ➜ Regex\n"                                   +
                 "This command evaluates the messages to skip, "           +
                 "then filters by user (if specified), "                   +
                 "and then by regex (if specified).\n\n"                   +
                 "Furthermore, **--around and --skip are mutually exclusive.**")]
    //[RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
    public async Task<Result<IMessage>> Clear
    (
        [Description("How many messages to delete. `--around` limits to 100.")]
        int messageCount = 5,
        
        [Option('s', "skip")]
        [Description("The amount of messages to skip.")]
        int? skip = null,
        
        [Option('u', "user")]
        [Description("The user to filter by.")]
        IUser? user = null,
        
        [Option('r', "regex")]
        [Description("The regex to filter by.")]
        string? pattern = null,
        
        [Option('a', "around")]
        [Description("Delete messages around the specified message.")]
        IMessage? message = null
        
        //TODO: Images
    )
    {
        if (skip is not null && message is not null)
            return await _channels.CreateMessageAsync(_context.ChannelID, "You can only specify --skip **or** --around.");
        
        if (message?.ID.Timestamp.AddDays(14) < DateTimeOffset.UtcNow)
            return await _channels.CreateMessageAsync(_context.ChannelID, "You can specify messages up to two weeks old.");
        
        var messageResult = await GetMessagesAsync(_context.ChannelID, message?.ID ?? default(Optional<Snowflake>), messageCount + (skip + 1 ?? 1)); 
            
        if (!messageResult.IsSuccess)
            return Result<IMessage>.FromError(messageResult.Error);
        
        var messages = messageResult.Entity.AsEnumerable();
        
        var twoWeeks = DateTimeOffset.UtcNow.AddHours(-(24 * 14 - 0.5));

        messages = messages.Where(m => m.ID.Timestamp > twoWeeks);

        messages = messages.Skip(skip + 1 ?? 1);

        if (user is not null)
            messages = messages.Where(x => x.Author.ID == user.ID);

        if (pattern is not null)
        {
            var filterRegex = new Regex(pattern);
            
            messages = messages.Where(x => filterRegex.IsMatch(x.Content));
        }
        
        messages = messages.Take(messageCount);
        
        var messageDeleteResult = await _channels.BulkDeleteMessagesAsync(_context.ChannelID, messages.Select(x => x.ID).ToArray());
        
        if (!messageDeleteResult.IsSuccess)
            return Result<IMessage>.FromError(messageDeleteResult.Error);

        var deleted = Math.Min(messageCount, messages.Count());
        
        var returnResult = await _channels.CreateMessageAsync(_context.ChannelID, $"{Emojis.DeleteEmoji} Deleted {deleted} message{(deleted == 1 ? "" : "s")}.");
        
        await Task.Delay(6000);

        await _channels.DeleteMessageAsync(_context.ChannelID, (_context as MessageContext)!.MessageID);
        await _channels.DeleteMessageAsync(_context.ChannelID, returnResult.Entity.ID);

        return returnResult;
    }

    /// <summary>
    /// Gets the messages from the specified channel.
    /// </summary>
    /// <param name="channelID">The ID of the channel to fetch messages from.</param>
    /// <param name="around">The ID of the message to fetch messages around</param>
    /// <param name="limit">The limit of messages. If around is not specified, and this is greater than 100, the request will be paginated.</param>
    /// <returns></returns>
    private async Task<Result<IReadOnlyList<IMessage>>> GetMessagesAsync(Snowflake channelID, Optional<Snowflake> around, int limit)
    {
        if (limit <= 100 || around.HasValue)
            return await _channels.GetChannelMessagesAsync(channelID, around, limit: limit);

        var messages = new List<IMessage>();
        
        var remaining = limit;
        var before    = default(Optional<Snowflake>);

        while (remaining > 0)
        {
            var fetchResult = await _channels.GetChannelMessagesAsync(channelID, before, limit: 100);
            
            if (fetchResult.IsSuccess)
            {
                messages.AddRange(fetchResult.Entity);
                remaining -= fetchResult.Entity.Count;
                before    = fetchResult.Entity.Last().ID;
                
                if (fetchResult.Entity.Count < 100)
                    break;
            }
            else
            {
                return fetchResult;
            }
        }
        
        return messages;
    }
}