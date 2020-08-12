using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot
{
    public class DMCommand : BaseCommandModule
    {
        [RequireOwner]
        [Command("DM")]
        public async Task DM(CommandContext ctx, ulong guildId, DiscordUser user, [RemainingText] string message)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);
            var member = guild.Members.Single(pair => pair.Key == user.Id);

            await member.Value.SendMessageAsync(message);
        }
        [RequireOwner]
        [Command("DM")]
        public async Task DM(CommandContext ctx, DiscordUser user, [RemainingText] string message)
        {
            var member = ctx.Guild.Members.Single(pair => pair.Key == user.Id);
            await member.Value.SendMessageAsync(message);
        }


        public static async Task DM(CommandContext ctx, DiscordUser user, DiscordEmbed message)
        {
            var member = ctx.Guild.Members.Single(pair => pair.Key == user.Id);
            try 
            {
                await member.Value.SendMessageAsync(embed: message);
            } catch(Exception e)
            {
                throw new InvalidOperationException();
            }
            
        }



    }
}
