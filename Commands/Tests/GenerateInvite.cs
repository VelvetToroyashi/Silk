using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot
{
    public class GenerateInvite : BaseCommandModule
    {
        [Command("GenInvite")]
        [Hidden]
        public async Task GenerateNewInvite(CommandContext ctx, [NotNull] string authKey , string ServerName, bool newInvite = false)
        {

            if (!authKey.Equals(AuthCommand.Key))
            {
                await ctx.RespondAsync("Invalid auth key!");
                await Task.Delay(3000);
                await ctx.Channel.GetMessagesAfterAsync(ctx.Message.Id, 1).Result[0].DeleteAsync();
                return;
            }
            
            var inviteServer = ctx.Client.Guilds.Values.Single(server => server.Name == ServerName);
            var invites = "Possible invites: ";
            foreach (var invite in inviteServer.GetInvitesAsync().Result)
                invites += $"discord.gg/{invite.Code} ,";
            AuthCommand.ClearKeyAsync();
            var DM = new DMCommand().DM(ctx, ctx.Client.GetUserAsync(209279906280898562).Result, $"Possible invites: ");


            await Task.CompletedTask;
        }
    }
}
