using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Silk.Extensions;

namespace Silk.Core.Discord.Utilities
{
    public static class EmbedHelper
    {
        public static DiscordEmbedBuilder CreateEmbed(string title, string description, DiscordColor color = default)
        {
            return new DiscordEmbedBuilder().WithTitle(title).WithDescription(description).WithColor(color);
        }

        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string Title, string Description)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle(Title)
                .WithDescription(Description);
        }


        public static DiscordEmbedBuilder CreateEmbed(
            CommandContext ctx, string title, string description,
            DiscordColor color)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .AddFooter(ctx);
        }

        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string description, DiscordColor color)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithDescription(description)
                .WithColor(color)
                .AddFooter(ctx);
        }
    }
}