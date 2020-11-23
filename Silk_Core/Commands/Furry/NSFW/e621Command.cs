using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using JetBrains.Annotations;
using SilkBot.Commands.Furry.Utilities;
using SilkBot.Utilities;

namespace SilkBot.Commands.Furry.NSFW
{
    [UsedImplicitly]
    [Category(Categories.Misc)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Cooldown(1, 10, CooldownBucketType.User)]
    public class e621Command : eBooruBaseCommand
    {
        public e621Command(HttpClient client) : base(client)
        {
            baseUrl = "https://e621.net/posts.json?tags=";
        }

        [Command("e621")]
        [Aliases("e6")]
        [Description("Lewd~ Get hot stuff of e621; requires channel to be marked as NSFW.")]
        [RequireNsfw]
        public override async Task Search(CommandContext ctx, int amount = 1, [RemainingText] string query = null)
        {
            if (query?.Split().Length > 5)
            {
                await ctx.RespondAsync("You can search 5 tags at a time!");
                return;
            }
            else if (amount > 7)
            {
                await ctx.RespondAsync("You can only request 10 images every 10 seconds.");
                return;
            }

            eBooruPostResult result = await DoQueryAsync(query);
            if (result is null || result.Posts.Count is 0)
            {
                await ctx.RespondAsync("Seems like nothing exists by that search! Sorry! :(");
                return;
            }

            List<Post> posts = await GetPostsAsync(result, amount, (int) ctx.Message.Id);
            foreach (Post post in posts)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                            .WithTitle(query)
                                            .WithDescription(
                                                $"[Direct Link]({post.File.Url})\nDescription: {post.Description.Truncate(200)}")
                                            .AddField("Score:", post.Score.Total.ToString())
                                            .AddField("Source:",
                                                GetSource(post.Sources.FirstOrDefault()?.ToString()) ??
                                                "No source available")
                                            .WithColor(DiscordColor.PhthaloBlue).WithImageUrl(post.File.Url)
                                            .WithFooter("Limit: 7 img / 30sec");
                await ctx.RespondAsync(embed: embed);
                await Task.Delay(300);
            }
        }
    }
}