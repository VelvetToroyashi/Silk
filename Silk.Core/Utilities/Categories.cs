using System.Collections.Generic;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace Silk.Core.Utilities
{
    public static class Categories
    {
        public const string
            Dev = CustomEmoji.Discord + " Developer",
            Mod = CustomEmoji.Staff + " Mod",
            General = "📁 General",
            Games = "🎮 Games",
            Misc = "💡 Misc",
            // Todo: Add Emoji's for Categories
            Server = CustomEmoji.Server + " Server",
            Roles = " Roles",
            Bot = CustomEmoji.Bot + " Bot",
            Economy = CustomEmoji.Money + " Economy";

        public static readonly IReadOnlyList<string> Order
            = new[] {Dev, General, Games, Misc, Mod, Server, Bot, Roles, Economy};
    }

    public static class CustomEmoji
    {
        public static readonly DiscordEmoji
            ECheck = Check.ToEmoji(),
            ECross = Cross.ToEmoji(),
            ELoading = Loading.ToEmoji(),
            EHelp = Help.ToEmoji(),
            EGitHub = GitHub.ToEmoji();


        public static DiscordEmoji ToEmoji(this string text)
        {
            Match match = Regex.Match(text.Trim(), @"^<?a?:?([a-zA-Z0-9_]+:[0-9]+)>?$");
            return DiscordEmoji.FromUnicode(match.Success ? match.Groups[1].Value : text.Trim());
        }


        public const string
            Check = "<:check:410612082929565696>",
            Cross = "<:cross:410612082988285952>",
            Loading = "<a:loading:410612084527595520>",
            Help = "<:help:438481218674229248>",
            Bot = "<:bot:777726275161817088>",
            Money = "<:money:777725904758505482>",
            Discord = "<:developer:777724802793996298>",
            GitHub = "<:github:409803419717599234>",
            Staff = "<:DiscordStaff:777722613966438442>",
            Server = "<:Server:787537636007870485>",
            Empty = "<:Empty:445680384592576514>";
    }
}