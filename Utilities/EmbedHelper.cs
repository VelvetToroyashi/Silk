using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SilkBot.Commands.Economy.Shop;
using SilkBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkBot
{
    public static class EmbedHelper
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

        public static DiscordEmbed GenerateShopUI(this DiscordEmbed embed, IEnumerable<IShopItem> items)
        {
            //If the embed contains any fields, re-render the embed without them.//
            if (embed.Fields.Any())
            {
                embed = new DiscordEmbedBuilder()
                    .WithAuthorExtension(embed.Author.Name, embed.Author.IconUrl.ToString())
                    .WithTitle(embed.Title)
                    .WithDescription(embed.Description)
                    .WithColor(embed.Color.Value);
            }
            var embedBuilder = new DiscordEmbedBuilder(embed);
            
            foreach (var item in items)
            {
                embedBuilder.AddField(item.Name, $"{item.Description}  - Price: ${item.Price} dollars.");
            }
            return embedBuilder.Build();
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
