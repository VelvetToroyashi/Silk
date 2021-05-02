using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Logic.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    [Hidden]
    public class NicknameCommand : BaseCommandModule
    {
        private readonly ILogger<NicknameCommand> _logger;

        public NicknameCommand(ILogger<NicknameCommand> logger)
        {
            _logger = logger;
        }

        [RequireFlag(UserFlag.Staff)]
        [Command("nickname")]
        [Aliases("nick")]
        [Description("Set your nickname on the current Guild")]
        public async Task SetNickName(CommandContext ctx, DiscordMember target, [RemainingText] string nick)
        {
            await ctx.Message.DeleteAsync();
            if (nick.Length > 32)
            {
                await ctx.RespondAsync("Nickname out of bounds! Limit: 32 characters");
                return;
            }

            try
            {
                await target.ModifyAsync(t => t.Nickname = nick);
            }
            catch (Exception)
            {
                await ctx.RespondAsync("Could not set nickname!");
                _logger.LogWarning($"Attempted to modify {target.Username} ({target.Nickname} -> {nick}), but an exception was thrown.");
            }
        }
    }
}