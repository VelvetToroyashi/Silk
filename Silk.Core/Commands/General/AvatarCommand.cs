using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class AvatarCommand : BaseCommandModule
    {
        [Command("avatar")]
        [Description("Show your, or someone else's avatar!")]
        public async Task GetAvatarAsync(CommandContext ctx, DiscordUser user)
        {
            DiscordEmbedBuilder embedBuilder = DefaultAvatarEmbed(ctx)
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithDescription($"{user.Mention}'s Avatar")
                .WithImageUrl(AvatarImageResizedUrl(user.AvatarUrl));
            
            await ctx.RespondAsync(embed: embedBuilder);
        }
        
        [Command("avatar")]
        
        public async Task GetAvatarAsync(CommandContext ctx, [RemainingText] string? user)
        {
            DiscordMember? u = ctx.Guild.Members.Values.FirstOrDefault(u => string.Equals(user, u.Username, StringComparison.OrdinalIgnoreCase));

            if (u is null)
                await ctx.RespondAsync("Sorry, I couldn't find anyone with a name matching the text provided.");
            else
            {
                await ctx.RespondAsync(embed: 
                    DefaultAvatarEmbed(ctx)
                        .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                        .WithDescription($"{u.Mention}'s Avatar")
                        .WithImageUrl(AvatarImageResizedUrl(u.AvatarUrl)));
            }
        }

        private static string AvatarImageResizedUrl(string avatarUrl) => avatarUrl.Replace("128", "4096&v=1");

        private static DiscordEmbedBuilder DefaultAvatarEmbed(CommandContext ctx) =>
            new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue)
                .WithFooter($"Silk! | Requested by {ctx.User.Username}/{ctx.User.Id}", ctx.User.AvatarUrl);
        
    }
}