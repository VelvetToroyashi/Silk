using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using SilkBot.Services;

namespace SilkBot.Commands
{
    public class CommandProcessorModule
    {
        private static ConcurrentDictionary<ulong, List<string>> disabledCommandsCache = new();
        private static ConcurrentDictionary<ulong, List<ulong>> whitelistedChannelsCache = new();
        private readonly PrefixCacheService _prefixCache;
        
        public CommandProcessorModule(PrefixCacheService prefixCache)
        {
            _prefixCache = prefixCache;
        }

        public static void DisableCommand(string command, ulong id)
        {
            disabledCommandsCache.TryGetValue(id, out var value);
            if (value is not null)
            {
                value.Add(command);
            }
            else
            {
                value = new();
                value.Add(command);
                disabledCommandsCache.TryAdd(id, value);
            }
        }
        public async Task OnMessageCreate(DiscordClient c, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            { 
                
                string? prefix = _prefixCache.RetrievePrefix(e.Guild?.Id) ?? string.Empty;
                CommandsNextExtension? cnext = c.GetCommandsNext();
                int commandLength =
                    e.MentionedUsers.Any(u => u.Id == c.CurrentUser.Id)
                        ? e.Message.GetMentionPrefixLength(c.CurrentUser)
                        : e.Message.GetStringPrefixLength(prefix);
                string? commandString = e.Message.Content.Substring(commandLength);
                
                if (e.Guild is not null &&
                    (disabledCommandsCache.GetValueOrDefault(e.Guild.Id)
                                          ?.Contains(commandString.Split(' ')?[0]) ?? false)) return;
                
                
                Command command = cnext.FindCommand(commandString, out string arguments);
                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);
                await cnext.ExecuteCommandAsync(context);
            });
        }
    }
}