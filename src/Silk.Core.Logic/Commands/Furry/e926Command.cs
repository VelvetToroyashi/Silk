using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Core.Logic.Commands.Furry.Types;

namespace Silk.Core.Logic.Commands.Furry
{
    [Category(Categories.Misc)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Cooldown(1, 15, CooldownBucketType.User)]
    public class e926Command : eBooruBaseCommand
    {
        public e926Command(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            baseUrl = "https://e926.net/posts.json?tags=";
        }

        [Command("e926")]
        [Aliases("e9")]
        [Description("SFW! Get cute stuff off none other than e926.net." +
                     "(See the [tags](https://e926.net/tags) section on e926.")]
        public override async Task Search(CommandContext ctx, int amount = 1, [RemainingText] string? query = null)
        {
            if (query?.Split().Length > 5)
            {
                await ctx.RespondAsync("You can search 5 tags at a time!");
                return;
            }
            if (amount > 7)
            {
                await ctx.RespondAsync("You can only request 7 images every 30 seconds.");
                return;
            }

            eBooruPostResult? result = await DoQueryAsync(query);
            if (result is null)
            {
                await ctx.RespondAsync("Seems like nothing exists by that search! Sorry! :(");
                return;
            }

            List<Post> posts = await GetPostsAsync(result, amount, (int) ctx.Message.Id);
            foreach (Post post in posts)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle(query)
                    .WithDescription($"[Direct Link](https://e926.net/posts/{post!.Id})\n" +
                                     $"Description: {post.Description.Truncate(200)}")
                    .AddField("Score:", post.Score.Total.ToString())
                    .WithColor(DiscordColor.PhthaloBlue)
                    .WithImageUrl(post.File.Url)
                    .WithFooter("Limit: 7 img / 15 sec");

                await ctx.RespondAsync(embed);
                await Task.Delay(300);
            }
        }

        [Command("e926")]
        public async Task Search(CommandContext ctx, [RemainingText] string? search) => await Search(ctx, 3, search);
    }
}