using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Results;
using Remora.Results;
using Silk.Shared.Constants;

namespace Silk.Core.Commands.Furry.Types
{
    public abstract class eBooruBaseCommand : CommandGroup
    {
        private protected readonly IDiscordRestChannelAPI _channelApi;
        private readonly           HttpClient             _client;
        private protected readonly ICommandContext        _context;
        private protected          string?                baseUrl;
        // Needed for e621.net //
        private protected string? username;

        public eBooruBaseCommand(HttpClient httpClientFactory, ICommandContext context, IDiscordRestChannelAPI channelApi)
        {
            _client = httpClientFactory;
            _context = context;
            _channelApi = channelApi;
        }

        /// <summary>
        ///     Execute the query from the specified booru site.
        /// </summary>
        /// <param name="amount">Amount of images to return.</param>
        /// <param name="query">Query sent to the booru site.</param>
        public abstract Task<Result> Search(int amount = 3, [Greedy] string? query = null);


        /// <summary>
        ///     Make a GET request to the booru site (e6/e9), and return the result.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private protected async Task<Result<eBooruPostResult?>> DoQueryAsync(string? query)
        {
            // Thanks to Spookdot on Discord for showing me this method existed. ~Velvet. //
            //var posts = await _client.GetFromJsonAsync<eBooruPostResult>($"{baseUrl}{query?.Replace(' ', '+')}");
            // It didn't work. :c //
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{query?.Replace(' ', '+')}");

            request.Headers.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);

            using HttpResponseMessage? result = await _client.SendAsync(request);

            if (!result.IsSuccessStatusCode)
                return Result<eBooruPostResult?>.FromError(new HttpResultError(result.StatusCode, "API is down."));

            var posts = JsonConvert.DeserializeObject<eBooruPostResult>(await result.Content.ReadAsStringAsync());

            for (var i = 0; i < posts!.Posts?.Count; i++)
                if (posts.Posts[i]?.File.Url is null || posts.Posts[i].File.Url.ToString() is "")
                    posts.Posts.Remove(posts.Posts[i]);

            return posts.Posts?.Count is 0 ? null : posts;
        }

        /// <summary>
        ///     Similar to <see cref="DoQueryAsync" /> but adds a specified API key when making a GET request.
        /// </summary>
        /// <param name="query">search query to put in the GET request.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="requireUsername">Add <see cref="username" /> to the HTTP header or not.</param>
        /// <returns></returns>
        private protected async Task<eBooruPostResult?> DoKeyedQueryAsync(string? query, string apiKey, bool requireUsername = false)
        {
            if (requireUsername)
                _ = username ?? throw new ArgumentNullException($"{nameof(username)} can't be null.");
            _ = apiKey ?? throw new ArgumentNullException($"{nameof(apiKey)} can't be null.");


            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl + query));

            byte[]? cred = Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{apiKey}");
            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(cred)}");
            request.Headers.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);

            // TODO: Log if API key is rejected.
            string result = await (await _client.SendAsync(request)).Content.ReadAsStringAsync();
            var posts = JsonConvert.DeserializeObject<eBooruPostResult>(result);

            for (var i = 0; i < posts!.Posts?.Count; i++)
                if (posts.Posts[i]?.File.Url is null || posts.Posts[i].File.Url.ToString() is "")
                    posts.Posts.Remove(posts.Posts[i]);
            // Still remove blank posts even after authenticating, in case they're blacklisted. //

            return posts.Posts?.Count is 0 ? null : posts;
        }

        /// <summary>
        ///     Get a set number of posts randomly from the list of available posts.
        /// </summary>
        /// <param name="resultPosts">e6/e9 post result.</param>
        /// <param name="amount">The amount of posts to return.</param>
        /// <param name="seed">The seed for the <see cref="Random" /> used to pick posts, preferably being a casted message Id.</param>
        /// <returns>A list of mostly random posts from a given post result.</returns>
        private protected IReadOnlyList<Post> GetRandomPosts(List<Post>? resultPosts, int amount, int seed = default)
        {
            if (resultPosts is null)
                return Array.Empty<Post>();

            var rand = new Random(seed);
            var posts = new List<Post?>();
            for (var i = 0; i < amount; i++)
            {
                if (resultPosts.Count is 0) break; // No results were returned. //
                int r = rand.Next(resultPosts.Count);
                if (resultPosts?[r] is null) // That post doesn't exist. //
                {
                    i--;
                    resultPosts.RemoveAt(r);
                    continue;
                }

                posts.Add(resultPosts[r]);
                resultPosts.RemoveAt(r);
            }

            return posts!;
        }

        private protected string? GetSource(string? source)
        {
            if (source is null) return null;
            return Regex.Match(source, @"[A-z]+\.[A-z]+\/").Value[..^1];
        }
    }
}