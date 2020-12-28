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
        public async Task GetAvatarAsync(CommandContext ctx, [Description("Test pog?")] DiscordUser user)
        {
            DiscordEmbedBuilder embedBuilder = DefaultAvatarEmbed(ctx)
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithDescription($"{user.Mention}'s Avatar")
                .WithImageUrl(AvatarImageResizedUrl(user.AvatarUrl));
            
            await ctx.RespondAsync(embed: embedBuilder);
        }
        
        [Command("avatar")]
        
        public async Task GetAvatarAsync(CommandContext ctx, [RemainingText] string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                await ctx.RespondAsync(embed: DefaultAvatarEmbed(ctx).WithImageUrl(AvatarImageResizedUrl(ctx.User.AvatarUrl)));
            }
            else
            {
                DiscordMember user = ctx.Guild.Members
                    .FirstOrDefault(m => m.Value.DisplayName.ToLower()
                        .Contains(text.ToLower())).Value;

                if (user is null)
                {
                    await ctx.RespondAsync("Sorry, I couldn't find anyone with a name matching the text provided.");
                }
                else
                {
                    await ctx.RespondAsync(embed: 
                        DefaultAvatarEmbed(ctx)
                            .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                            .WithDescription($"{user.Mention}'s Avatar")
                            .WithImageUrl(AvatarImageResizedUrl(user.AvatarUrl)));
                }
            }
        }

        private static string AvatarImageResizedUrl(string avatarUrl) => avatarUrl.Replace("128", "4096");

        private static DiscordEmbedBuilder DefaultAvatarEmbed(CommandContext ctx)
        {
            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue);
        }
    }
}