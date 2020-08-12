using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;

namespace SilkBot
{
    public static class EmbedGenerator
    {
        public static DiscordEmbed CreateEmbed(CommandContext ctx, string Title, string Description)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle(Title)
                .WithDescription(Description)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

        }
        public static DiscordEmbed CreateEmbed(DiscordClient discordClient, string Title, string Description)
        {
            return new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle(Title)
                .WithDescription(Description)
                .WithFooter("Silk", discordClient.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

        }
        public static DiscordEmbed CreateEmbed(CommandContext ctx, string Title, string Description, DiscordColor Color)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(Color)
                .WithTitle(Title)
                .WithDescription(Description)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

        }
    }
}
