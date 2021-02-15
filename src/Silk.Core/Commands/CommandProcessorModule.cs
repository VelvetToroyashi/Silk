using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Commands
{
    public class CommandProcessorModule
    {
        private static readonly ConcurrentDictionary<ulong, List<string>> disabledCommandsCache = new();
        private static ConcurrentDictionary<ulong, List<ulong>> whitelistedChannelsCache = new();
        private readonly IPrefixCacheService _prefixCache;

        public CommandProcessorModule(IPrefixCacheService prefixCache)
        {
            _prefixCache = prefixCache;
        }

        public static void DisableCommand(string command, ulong guildId)
        {
            disabledCommandsCache.TryGetValue(guildId, out var guildDisabledCommands);
            if (guildDisabledCommands is not null)
            {
                guildDisabledCommands.Add(command);
            }
            else
            {
                guildDisabledCommands = new();
                guildDisabledCommands.Add(command);
                disabledCommandsCache.TryAdd(guildId, guildDisabledCommands);
            }
        }
        public async Task OnMessageCreate(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Author == c.CurrentUser) return;
            _ = Task.Run(async () =>
            {

                string? prefix = _prefixCache.RetrievePrefix(e.Guild?.Id) ?? string.Empty;

                CommandsNextExtension? cnext = c.GetCommandsNext();
                int commandLength =
                    e.MentionedUsers.Any(u => u.Id == c.CurrentUser.Id)
                        ? e.Message.GetMentionPrefixLength(c.CurrentUser)
                        : e.Message.GetStringPrefixLength(prefix);
                string? commandString = e.Message.Content.Substring(commandLength);
                string? split = commandString?.Split()?[0];
                if (e.Guild is not null &&
                    (disabledCommandsCache.GetValueOrDefault(e.Guild.Id)?.Contains(split) ?? false)) return;

                Command command = cnext.FindCommand(commandString, out string arguments);

                if (command is null)
                    throw new CommandNotFoundException(commandString);

                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);

                await cnext.ExecuteCommandAsync(context);
            });
        }
    }
}