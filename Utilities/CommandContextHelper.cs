using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.Utilities
{
    public static class CommandContextHelper
    {
        public static (string DisplayName, string Url) GetAuthor(this CommandContext ctx) => (ctx.Member.DisplayName, ctx.Member.AvatarUrl);
        public static DiscordEmbedBuilder WithAuthorExtension(this DiscordEmbedBuilder builder, string name, string avatarUrl) =>
            builder.WithAuthor(name, iconUrl: avatarUrl);
        public static DiscordEmbedBuilder AddFooter(this DiscordEmbedBuilder builder, CommandContext ctx) =>
            builder.WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            
        

    }
}
