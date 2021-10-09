using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Services.Interfaces;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
    /// <summary>
    ///     Utility class for anti-invite functionality.
    /// </summary>
    public sealed class AntiInviteHelper
	{
		private readonly ILogger<AntiInviteHelper> _logger;
		private readonly IMediator _mediator;
		private readonly IInfractionService _infractions;
		private readonly DiscordClient _client;

		public AntiInviteHelper(ILogger<AntiInviteHelper> logger, IMediator mediator, IInfractionService infractions, DiscordClient client)
		{
			_logger = logger;
			_mediator = mediator;
			_infractions = infractions;
			_client = client;
		}

        /// <summary>
        ///     Regex to match discord invites using discord's main invite URL (discord.gg)
        /// </summary>
        public static Regex LenientRegexPattern { get; } = new(@"discord.gg\/([A-z]*-*[0-9]*){2,}", FlagConstants.RegexFlags);

        /// <summary>
        ///     A more aggressive regex to match anything that could be considered an invite/attempt to circumvent <see cref="LenientRegexPattern" />.
        ///     Includes, but is not limited to discord.gg, discord.com/invite, and disc.gg
        /// </summary>
        public static Regex AggressiveRegexPattern { get; } = new(@"disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-]{2,})", FlagConstants.RegexFlags);

        /// <summary>
        ///     Checks if a <see cref="DiscordMessage" /> has a valid <see cref="DiscordInvite" />.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <param name="config">The configuration of the guild the message was sent on.</param>
        /// <param name="invite">The invite that was matched, if any.</param>
        /// <returns>Whether further action should be taken</returns>
        public bool CheckForInvite(DiscordMessage message, GuildModConfigEntity config, out string invite)
		{
			invite = "";

			if (config is null) return false;

			if (!config.BlacklistInvites) return false;
			if (message.Channel.IsPrivate) return false;
			if (message.Author.IsBot) return false;

			Regex scanPattern = config.UseAggressiveRegex ? AggressiveRegexPattern : LenientRegexPattern;
			Match match = scanPattern.Match(message.Content);

			invite = match.Groups.Values.Last().Captures.FirstOrDefault()?.Value ?? "";

			return match.Success;
		}

        /// <summary>
        ///     Checks if a suspected <see cref="DiscordInvite" /> is blacklisted.
        /// </summary>
        /// <param name="client">A client object to make API calls with.</param>
        /// <param name="message">The message to check.</param>
        /// <param name="config">The guild configuration, to determine whether an API call should be made.</param>
        /// <param name="invite">The invite to check.</param>
        /// <returns>Whether Auto-Mod should progress with the infraction steps regarding invites.</returns>
        public async Task<bool> IsBlacklistedInvite(DiscordMessage message, GuildModConfigEntity config, string invite)
		{
			if (config is null) return false;
			if (!config.ScanInvites) return config.AllowedInvites.All(inv => inv.VanityURL != invite);

			var blacklisted = true;
			try
			{
				DiscordInvite apiInvite = await _client.GetInviteByCodeAsync(invite);

				if (apiInvite.Guild.Id != message.Channel.GuildId)
				{
					blacklisted = config.AllowedInvites.All(inv => apiInvite.Guild.Id != inv.InviteGuildId);
				}
				else
				{
					blacklisted = false;
					_logger.LogTrace(EventIds.AutoMod, "Matched invite points to current guild; skipping");
				}
			}
			catch (NotFoundException) // Discord throws 404 if you ask for an invalid invite. i.e. Garbage behind a legit code. //
			{
				_logger.LogTrace(EventIds.AutoMod, "Matched invalid or corrupt invite");
			}
			return blacklisted;
		}

        /// <summary>
        ///     Attempts to infract a member for posting an invite.
        /// </summary>
        /// <param name="message"></param>
        public async Task TryAddInviteInfractionAsync(DiscordMessage message, GuildModConfigEntity config)
		{
			UserEntity? user = await _mediator.Send(new GetOrCreateUserRequest(message.Channel.Guild.Id, message.Author.Id));

			if (user.Flags.HasFlag(UserFlag.InfractionExemption))
				return;

			if (config.DeleteMessageOnMatchedInvite)
				await message.DeleteAsync("[AutoMod] Detected a blacklisted invite.");
			
			if (config.WarnOnMatchedInvite)
				await _infractions.StrikeAsync(message.Author.Id, message.Channel.Guild.Id, message.GetClient().CurrentUser.Id, $"Posted an invite in {message.Channel.Mention}", config.AutoEscalateInfractions);
		}
	}
}