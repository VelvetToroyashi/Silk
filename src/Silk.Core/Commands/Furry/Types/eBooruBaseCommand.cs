using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Furry.Utilities
{
    public abstract class eBooruBaseCommand : BaseCommandModule
    {
        private protected string? baseUrl;
        // Needed for e621.net //
        private protected string? username;

        private readonly HttpClient _client;

        public eBooruBaseCommand(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateSilkClient();
        }

        /// <summary>
        /// Execute the query from the specified booru site.
        /// </summary>
        /// <param name="ctx">The command context to execute this command in.</param>
        /// <param name="amount">Amount of images to return.</param>
        /// <param name="query">Query sent to the booru site.</param>
        public abstract Task Search(CommandContext ctx, int amount = 1, [RemainingText] string? query = null);

        /// <summary>
        /// Make a GET request to the booru site (e6/e9), and return the result.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private protected async Task<eBooruPostResult?> DoQueryAsync(string? query)
        {
            // Thanks to Spookdot on Discord for showing me this method existed. ~Velvet. //
            //var posts = await _client.GetFromJsonAsync<eBooruPostResult>($"{baseUrl}{query?.Replace(' ', '+')}");
            // It didn't work. :c //
            string result = await _client.GetStringAsync($"{baseUrl}{query?.Replace(' ', '+')}");
            var posts = JsonConvert.DeserializeObject<eBooruPostResult>(result);

            for (var i = 0; i < posts.Posts?.Count; i++)
                if (posts.Posts[i]?.File.Url is null || posts.Posts[i].File.Url.ToString() is "")
                    posts.Posts.Remove(posts.Posts[i]);

            return posts.Posts?.Count is 0 ? null : posts;
        }

        /// <summary>
        /// Similar to <see cref="DoQueryAsync"/> but adds a specified API key when making a GET request.
        /// </summary>
        /// <param name="query">search query to put in the GET request.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="requireUsername">Add <see cref="username"/> to the HTTP header or not.</param>
        /// <returns></returns>
        private protected async Task<eBooruPostResult?> DoKeyedQueryAsync(string? query, string apiKey, bool requireUsername = false)
        {
            if (requireUsername)
                _ = username ?? throw new ArgumentNullException($"{nameof(username)} can't be null.");
            _ = apiKey ?? throw new ArgumentNullException($"{nameof(apiKey)} can't be null.");


            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl + query));

            var cred = Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{apiKey}");
            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(cred)}");
            // TODO: Log if API key is rejected.
            string result = await _client.Send(request).Content.ReadAsStringAsync();
            var posts = JsonConvert.DeserializeObject<eBooruPostResult>(result);

            for (var i = 0; i < posts.Posts?.Count; i++)
                if (posts.Posts[i]?.File.Url is null || posts.Posts[i].File.Url.ToString() is "")
                    posts.Posts.Remove(posts.Posts[i]);
            // Still remove blank posts even after authenticating, in case they're blacklisted. //

            return posts.Posts?.Count is 0 ? null : posts;
        }

        /// <summary>
        /// Get a set number of posts randomly from the list of available posts.
        /// </summary>
        /// <param name="post">e6/e9 post result.</param>
        /// <param name="amount">The amount of posts to return.</param>
        /// <param name="seed">The seed for the <see cref="Random"/> used to pick posts, preferably being a casted message Id.</param>
        /// <returns>A list of mostly random posts from a given post result.</returns>
        private protected async Task<List<Post?>> GetPostsAsync(eBooruPostResult? post, int amount, int seed = default)
        {
            var rand = new Random(seed);
            var posts = new List<Post?>();
            for (var i = 0; i < amount; i++)
            {
                if (post?.Posts?.Count is 0) break;
                int r = rand.Next(post?.Posts?.Count ?? 0);
                if (post?.Posts?[i] is null) continue;
                posts.Add(post.Posts?[r]);
                post.Posts?.RemoveAt(r);
            }
            return posts;
        }

        private protected string? GetSource(string? source)
        {
            if (source is null) return null;
            return Regex.Match(source, @"[A-z]+\.[A-z]+\/").Value[..^1];
        }
    }
}