using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;
using Silk.Core.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Core.AutoMod
{
	/// <summary>
	///     An AutoMod feature that automatically re-applies mutes when members rejoin a guild.
	/// </summary>
	public sealed class AutoModMuteApplier //: IResponder<IGuildMemberAdd>
    {
        private readonly IInfractionService          _infractions;
        private readonly ILogger<AutoModMuteApplier> _logger;
        private readonly IDiscordRestUserAPI         _users;
        public AutoModMuteApplier(IInfractionService infractions, ILogger<AutoModMuteApplier> logger, IDiscordRestUserAPI users)
        {
            _users = users;
            _logger = logger;
            _infractions = infractions;
        }

        public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.User.IsDefined())
                return Result.FromSuccess(); // ??? 

            ulong guild = gatewayEvent.GuildID.Value;
            ulong member = gatewayEvent.User.Value.ID.Value;

            bool isMuted = await _infractions.IsMutedAsync(member, guild);

            Result<IUser> automodRes = await _users.GetCurrentUserAsync(ct);

            if (!automodRes.IsSuccess)
                return Result.FromError(automodRes.Error);

            ulong automod = automodRes.Entity.ID.Value;

            if (!isMuted)
                return Result.FromSuccess();

            await _infractions.MuteAsync(member, guild, automod, "Re-applied active mute on join.", updateExpiration: false);
            await _infractions.AddNoteAsync(member, guild, automod, $"{StringConstants.AutoModMessagePrefix} Automatically re-applied mute for {member} on rejoin.");

            return Result.FromSuccess();
        }
    }
}