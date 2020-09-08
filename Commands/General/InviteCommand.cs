using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    public class InviteCommand : BaseCommandModule
    {
        [Command("Invite")]
        [HelpDescription("Gives you the Outh2 code to invite me to your server!")]
        public async Task Invite(CommandContext ctx)
        {
            var Oauth2 = "https://discord.com/api/oauth2/authorize?client_id=721514294587424888&permissions=502656214&scope=bot";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithDescription($"You can invite me with [this Oauth2]({Oauth2}) Link!")
                .WithFooter($"Silk ", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now));
        }

    }
}