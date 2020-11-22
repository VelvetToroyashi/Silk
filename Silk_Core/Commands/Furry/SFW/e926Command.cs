using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SilkBot.Utilities;

namespace SilkBot.Commands.Furry.SFW
{
    [Category(Categories.Misc)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Cooldown(1, 30, CooldownBucketType.User)]
    [UsedImplicitly]
    public class e926Command : BaseCommandModule
    {
        private const string BASE_ADDRESS = "https://e926.net/posts.json?tags=";
        private const string EMPTY_POSTS = "{\"posts\":[]}";
        private readonly HttpClient _client;
        private bool _timedOut;

        public e926Command(HttpClient client)
        {
            _client = client;
        }


        private readonly DiscordEmbed _timedOutEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.DarkRed).WithDescription("Sorry, but it seems like the API is taking longer than usual!");
        


        [Command("e926")]
        [Aliases("e9")]
        [Description("SFW! Get cute stuff off none other than e926.net." +
                     "(See the[tags](https://e926.net/tags) section on e926.")]
        public async Task Search(CommandContext ctx, int amount = 1, [RemainingText]string tags = "")
        {
            if (tags.Split(' ').Length > 5)
            {
                await ctx.RespondAsync("You can only search five tags at a time!");
                return;
            }

            if (amount is > 10)
            {
                await ctx.RespondAsync("You can only search for 10 images at a time!");
                return;
            }

            var tcs = new CancellationTokenSource();
            tcs.CancelAfter(4000); 
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + tags.Replace(' ', '+'), tcs.Token).ContinueWith(t => { _timedOut = t.IsCanceled; return t.Result; });
            if (_timedOut) { ctx.RespondAsync(embed: _timedOutEmbed); return; }

            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode is HttpStatusCode.NotFound || content == EMPTY_POSTS)
            {
                ctx.RespondAsync("Query returned 404! Nothing exists with those tags :(");
                return;
            }

            var result = JsonConvert.DeserializeObject<e926Result>(content);
            List<Post> postsList = new ();
            var ran = new Random((int)ctx.Message.Id);

            for (int i = 0; i < amount; i++)
            {
                int index = ran.Next(result.Posts.Count);
                postsList.Add(result.Posts[index]);
                result.Posts.RemoveAt(index);
            }

            foreach (var post in postsList)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.CornflowerBlue)
                    .WithTitle(tags)
                    .WithDescription($"[Direct link:]({post.File.Url})\n" +
                                     $"{post.FavCount} likes\n" +
                                     (post.Sources.Count is not 0 ? $"[Source]({post.Sources[0]})" : string.Empty))
                    .WithImageUrl(post.File.Url);
                await ctx.RespondAsync(embed: embed);
            }
        }
        
    }
}
