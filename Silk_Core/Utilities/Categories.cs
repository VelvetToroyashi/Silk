using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SilkBot.Utilities
{
    public static class Categories
    {
        public const string
            Dev = CustomEmoji.Discord + " Developer",
            Mod = CustomEmoji.Staff + " Mod",
            General = "📁 General",
            Games = CustomEmoji.GameCube + " Games",
            Misc = "💡 Misc";

        public static readonly IReadOnlyList<string> Order
            = new string[] { Dev, General, Games, Misc, Mod };
    }
    /// <summary>
    /// All custom emoji that this bot has access to from the Pac-Man support server.
    /// </summary>
    public static class CustomEmoji
    {
        public static readonly DiscordEmoji
            ECheck = Check.ToEmoji(),
            ECross = Cross.ToEmoji(),
            ELoading = Loading.ToEmoji(),
            EHelp = Help.ToEmoji(),
            EGitHub = GitHub.ToEmoji(),
            EBlobDance = BlobDance.ToEmoji();


        public static DiscordEmoji ToEmoji(this string text)
        {
            var match = Regex.Match(text.Trim(), @"^<?a?:?([a-zA-Z0-9_]+:[0-9]+)>?$");
            return DiscordEmoji.FromUnicode(match.Success ? match.Groups[1].Value : text.Trim());
        }


        public const string
            Check = "<:check:410612082929565696>",
            Cross = "<:cross:410612082988285952>",
            Loading = "<a:loading:410612084527595520>",
            PacMan = "<a:pacman:409803570544902144>",
            Help = "<:help:438481218674229248>",
            BlobDance = "<a:danceblob:751079963473477693>",
            GameCube = "<:gamecube:761373355742986290>",

            Discord = "<:discord:409811304103149569>",
            GitHub = "<:github:409803419717599234>",
            Staff = "<:staff:412019879772815361>",
            Thinkxel = "<:thinkxel:409803420308996106>",
            Empty = "<:Empty:445680384592576514>";


    }
}
