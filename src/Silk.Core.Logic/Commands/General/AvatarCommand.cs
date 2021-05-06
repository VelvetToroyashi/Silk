using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Logic.Commands.General
{
    [Category(Categories.General)]
    public class AvatarCommand : BaseCommandModule
    {
        [Command("avatar")]
        public async Task GetAvatar(CommandContext ctx, DiscordMember member)
        {
            await GetAvatarAsync(ctx, member);
        }

        [Command("avatar")]
        [Description("Show your, or someone else's avatar!")]
        public async Task GetAvatarAsync(CommandContext ctx, DiscordUser user)
        {
            DiscordEmbedBuilder embedBuilder = DefaultAvatarEmbed(ctx)
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithDescription($"{user.Mention}'s Avatar")
                .WithImageUrl(AvatarImageResizedUrl(user.AvatarUrl));

            await ctx.RespondAsync(embedBuilder);
        }

        [Command]
        [Description("Show your, or someone else's avatar!")]
        [Aliases("av")]
        public async Task Avatar(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);
            builder.WithEmbed(DefaultAvatarEmbed(ctx)
                .WithTitle("Your Avatar!")
                .WithImageUrl(AvatarImageResizedUrl(ctx.User.AvatarUrl)));
            await ctx.RespondAsync(builder);
        }

        [Command("avatar")]
        public async Task GetAvatarAsync(CommandContext ctx, [RemainingText] string user)
        {

            DiscordMember? userObj = ctx.Guild.Members.Values.FirstOrDefault(u => u.Username.Contains(user, StringComparison.OrdinalIgnoreCase));

            if (userObj is null)
            {
                await ctx.RespondAsync("Sorry, I couldn't find anyone with a name matching the text provided.");
            }
            else
            {
                await ctx.RespondAsync(
                    DefaultAvatarEmbed(ctx)
                        .WithDescription($"{userObj.Mention}'s Avatar")
                        .WithImageUrl(AvatarImageResizedUrl(userObj.AvatarUrl)));
            }
        }

        private static string AvatarImageResizedUrl(string avatarUrl)
        {
            return avatarUrl.Replace("128", "4096&v=1");
        }

        private static DiscordEmbedBuilder DefaultAvatarEmbed(CommandContext ctx)
        {
            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithFooter($"Silk! | Requested by {ctx.User.Username}/{ctx.User.Id}", ctx.User.AvatarUrl);
        }
    }
}