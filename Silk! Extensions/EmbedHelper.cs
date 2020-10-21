using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SilkBot.Utilities
{
    public static class EmbedHelper
    {
        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string Title, string Description) =>
            new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle(Title)
                .WithDescription(Description)
                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);



        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string title, string description, DiscordColor color) =>
            new DiscordEmbedBuilder()
            .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(color)
            .AddFooter(ctx);
        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string description, DiscordColor color) =>
            new DiscordEmbedBuilder()
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithDescription(description)
            .WithColor(color)
            .AddFooter(ctx);


    }
}
