#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

#endregion

namespace Silk.Extensions;

public static class CommandContextExtensions
{
    public static (string DisplayName, string Url) GetAuthor(this CommandContext ctx) => (ctx.Member.DisplayName, ctx.Member.AvatarUrl);

    public static DiscordEmbedBuilder WithAuthorExtension(
        this DiscordEmbedBuilder builder, string name,
        string                   avatarUrl) =>
        builder.WithAuthor(name, iconUrl: avatarUrl);

    public static DiscordEmbedBuilder AddFooter(this DiscordEmbedBuilder builder, CommandContext ctx) => builder;

    public static IEnumerable<DiscordMember> GetMemberByName(this CommandContext ctx, string input)
    {
        return ctx.Guild.Members
                  .Where(member => member.Value.DisplayName.ToLowerInvariant()
                                         .Contains(input.ToLowerInvariant()))
                  .Select(m => m.Value);
    }

    public static IEnumerable<DiscordMember> GetUserByName(this CommandContext ctx, string input)
    {
        IEnumerable<DiscordMember> members = ctx.Client.Guilds.SelectMany(g => g.Value.Members.Values);
        return members.Where(m => m.Username.ToLower().Contains(input.ToLower()) && !m.IsBot)
                      .Distinct(new DiscordMemberComparer());
    }

    public static string GetBotUrl(this CommandContext ctx) => $"https://discord.com/users/{ctx.Client.CurrentUser.Id}";
}

public class DiscordMemberComparer : IEqualityComparer<DiscordMember>
{
    public bool Equals([AllowNull] DiscordMember x, [AllowNull] DiscordMember y) => x.Id == y.Id;

    public int GetHashCode([DisallowNull] DiscordMember obj) => obj == null ? 0 : (int)obj.Id;
}