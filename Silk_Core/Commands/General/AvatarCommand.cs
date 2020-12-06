using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class AvatarCommand : BaseCommandModule
    {
        [Command("avatar")]
        public async Task GetAvatarAsync(CommandContext ctx, [Description("Test pog?")] DiscordUser user)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                                               .WithColor(DiscordColor.CornflowerBlue)
                                               .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.User.AvatarUrl)
                                               .WithDescription($"{user.Mention}'s Avatar")
                                               .WithImageUrl(UpsizeAvatarUrl(user.AvatarUrl));
            
            await ctx.RespondAsync(embed: embedBuilder);
        }

        [Command("avatar")]
        public async Task GetAvatarAsync(CommandContext ctx, DiscordMember m) =>
            await GetAvatarAsync(ctx, (DiscordUser) m).ConfigureAwait(false);
        
        //[Command("avatar")]
        [Description("Show your, or someone else's avatar!")]
        public async Task GetAvatarAsync(CommandContext ctx, [RemainingText] string? text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                await ctx.RespondAsync(embed: DefaultAvatarEmbed().WithImageUrl(UpsizeAvatarUrl(ctx.User.AvatarUrl)));
            }
            else
            {
                DiscordMember user = ctx.Guild.Members
                    .FirstOrDefault(m => m.Value.DisplayName.ToLower()
                        .Contains(text.ToLower())).Value;

                if (user is null)
                {
                    await ctx.RespondAsync("Sorry, I couldn't find anyone with a name matching the text provided.").ConfigureAwait(false);
                }
                else
                {
                    await ctx.RespondAsync(embed: 
                        DefaultAvatarEmbed()
                            .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                            .WithDescription($"{user.Mention}'s Avatar")
                            .WithImageUrl(UpsizeAvatarUrl(user.AvatarUrl))).ConfigureAwait(false);
                }
            }
        }

        private static string UpsizeAvatarUrl(string avatarUrl) => avatarUrl.Replace("128", "4096");

        private static DiscordEmbedBuilder DefaultAvatarEmbed() => new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue);
        
    }
}