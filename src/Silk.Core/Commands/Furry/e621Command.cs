using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Core.Commands.Furry.Types;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Shared.Configuration;

namespace Silk.Core.Commands.Furry
{
	[Utilities.HelpFormatter.Category(Categories.Misc)]
	//[ModuleLifespan(ModuleLifespan.Transient)]
	//[Cooldown(1, 10, CooldownBucketType.User)]
	public class e621Command : eBooruBaseCommand
	{
		private readonly SilkConfigurationOptions _options;

		public e621Command(
			HttpClient client,
			IOptions<SilkConfigurationOptions> options,
			ICommandContext context,
			IDiscordRestChannelAPI channel)
			: base(client, context, channel)
		{
			baseUrl = "https://e621.net/posts.json?tags=";
			_options = options.Value;
			username = _options.E621.Username;
		}

		//[RequireNsfw]
		[Command("e621", "e6")]
		[Description("Lewd~ Get hot stuff of e621; requires channel to be marked as NSFW.")]
		public override async Task<Result> Search(int amount = 3, string? query = null)
		{
			if (query?.Split().Length > 5)
				return Result.FromError(new ArgumentOutOfRangeError("You can search 5 tags at a time!"));

			if (amount > 10)
				return Result.FromError(new ArgumentOutOfRangeError("You can only request 10 images every 10 seconds."));

			Result<eBooruPostResult?> result;
			if (string.IsNullOrWhiteSpace(username))
				result = await DoQueryAsync(query); // May return empty results locked behind API key //
			else
				result = await DoKeyedQueryAsync(query, _options.E621.ApiKey, true);

			if (!result.IsSuccess)
			{
				var errorResult = await _channelApi.CreateMessageAsync(_context.ChannelID, result.Error!.Message);
				return errorResult.IsSuccess
					? Result.FromSuccess()
					: Result.FromError(errorResult.Error);
			}

			var booruPosts = result.Entity?.Posts;

			if (booruPosts is null || booruPosts.Count is 0)
				return Result.FromError(new ArgumentOutOfRangeError("No results found."));

			IReadOnlyList<Post> posts = GetRandomPosts(booruPosts, amount, (int)((_context as MessageContext)?.MessageID.Value ?? 0));
			List<IEmbed> embeds = posts.Select(post =>
					new Embed()
					{
						Title = query!,
						Description = $"[Direct Link]({post!.File.Url})\nDescription: {post!.Description.Truncate(200)}",
						Colour = Color.RoyalBlue,
						Image = new EmbedImage(post.File.Url.ToString()),
						Fields = new IEmbedField[]
						{
							new EmbedField("Score:", post.Score.Total.ToString(), true),
							new EmbedField("Source:", GetSource(post.Sources.FirstOrDefault()?.ToString()) ?? "No source available", true)
						}
					}
				)
				.Cast<IEmbed>()
				.ToList();

			var send = await _channelApi.CreateMessageAsync(_context.ChannelID, embeds: embeds);

			return send.IsSuccess
				? Result.FromSuccess()
				: Result.FromError(send.Error);
		}
	}
}