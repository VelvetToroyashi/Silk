using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Core.Services.Data;

namespace Silk.Core.Responders
{
    public class GuildJoinedCacherResponder : IResponder<IGuildCreate>
    {
        private readonly GuildCacherService _guildCacherService;
        public GuildJoinedCacherResponder(GuildCacherService guildCacherService) => _guildCacherService = guildCacherService;

        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
        {
            if (gatewayEvent.IsUnavailable.IsDefined(out bool unavailable) && unavailable)
                return Result.FromSuccess(); //???

            if (_guildCacherService.IsNewGuild(gatewayEvent.ID))
            {
                //await GreetGuildAsync(gatewayEvent);
            }

            if (!gatewayEvent.Members.IsDefined())
                return Result.FromError(new InvalidOperationError("Guild did not contain any members."));

            return await _guildCacherService.CacheGuildAsync(gatewayEvent.ID, gatewayEvent.Members.Value);
        }
    }
}