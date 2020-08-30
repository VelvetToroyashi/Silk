using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Miscellaneous
{
    public class Nickname : BaseCommandModule
    {
        [Command("nickname")]
        [Aliases("nick")]
        public async Task SetNickName(CommandContext ctx, DiscordMember target, [RemainingText] string nick)
        {
            await ctx.Message.DeleteAsync();
            if(nick.Length > 32)
            {
                await ctx.RespondAsync("Nickname out of bounds! Limit: 32 characters");
                return;
            }
            try
            {
                await target.ModifyAsync(t => t.Nickname = nick);
            }
            catch(Exception e)
            {
                await ctx.RespondAsync("Could not set nickname!");
                ctx.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Error, "Silk!", e.Message, DateTime.Now, e);
            }
            
        }
    }
}
