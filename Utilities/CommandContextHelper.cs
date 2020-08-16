using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static IEnumerable<DiscordMember> GetMemberByName(this CommandContext ctx, string input) =>
             ctx.Guild.Members
            .Where(member => member.Value.DisplayName.ToLowerInvariant()
            .Contains(input.ToLowerInvariant())).Select(m => m.Value);
        

    }
}
