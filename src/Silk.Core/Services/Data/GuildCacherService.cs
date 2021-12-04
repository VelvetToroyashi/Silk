using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Data
{
    public class GuildCacherService
    {
        private const string GuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs.\n\n" +
                                                        $"I respond to mentions and `{StringConstants.DefaultCommandPrefix}` by default, "      +
                                                        $"but you can change that with `{StringConstants.DefaultCommandPrefix}prefix`\n\n"      +
                                                        "There's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!";
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI   _guildApi;

        /// <summary>
        ///     A collection of guilds known to be already joined. Populated on READY gateway event.
        /// </summary>
        private readonly HashSet<Snowflake> _knownGuilds = new();

        private readonly ILogger<GuildCacherService> _logger;

        private readonly IMediator _mediator;

        private readonly IEmbed _onGuildJoinEmbed = new Embed(
                                                              Title: "Thank you for adding me!",
                                                              Description: GuildJoinThankYouMessage,
                                                              Colour: Color.Gold);
        private readonly IDiscordRestUserAPI _userApi;

        /// <summary>
        ///     Required permissions to send a welcome message to a new guild.
        /// </summary>
        private readonly DiscordPermissionSet _welcomeMessagePermissions = new(DiscordPermission.SendMessages, DiscordPermission.EmbedLinks);

        public GuildCacherService
        (
            IMediator                   mediator,
            IDiscordRestUserAPI         userApi,
            IDiscordRestGuildAPI        guildApi,
            IDiscordRestChannelAPI      channelApi,
            ILogger<GuildCacherService> logger
        )
        {
            _mediator = mediator;
            _userApi = userApi;
            _guildApi = guildApi;
            _channelApi = channelApi;
            _logger = logger;
        }

        public async Task<Result> GreetGuildAsync(IGuild guild)
        {
            //It's worth noting that there's a chance that one could pass
            //a guild fetched from REST here, which typically doesn't have
            //channels defined, which is a big issue, but that's on the caller

            Result<IUser> currentUserResult = await _userApi.GetCurrentUserAsync();

            if (!currentUserResult.IsSuccess)
                return Result.FromError(currentUserResult.Error);

            Result<IGuildMember> currentMemberResult = await _guildApi.GetGuildMemberAsync(guild.ID, currentUserResult.Entity.ID);

            if (!currentMemberResult.IsSuccess)
                return Result.FromError(currentMemberResult.Error);

            IGuildMember? currentMember = currentMemberResult.Entity;
            IUser? currentUser = currentMember.User.Value;

            IReadOnlyList<IChannel>? channels = guild.Channels.Value;

            IChannel? availableChannel = null;
            foreach (var channel in channels)
            {
                if (channel.Type is not ChannelType.GuildText)
                    continue;

                IReadOnlyList<IPermissionOverwrite>? overwrites = channel.PermissionOverwrites.Value;

                IDiscordPermissionSet? permissions = DiscordPermissionSet.ComputePermissions(
                                                                                             currentUser.ID,
                                                                                             guild.Roles.Single(r => r.ID == guild.ID),
                                                                                             guild.Roles.Where(r => currentMember.Roles.Contains(r.ID)).ToArray(),
                                                                                             overwrites);

                if (permissions.HasPermission(DiscordPermission.SendMessages) && permissions.HasPermission(DiscordPermission.EmbedLinks))
                {
                    availableChannel = channel;
                    break;
                }
            }

            if (availableChannel is null)
            {
                _logger.LogInformation("Unable to find suitable channel. Guild likely has verification.");
            }
            else
            {
                Result<IMessage> send = await _channelApi.CreateMessageAsync(availableChannel.ID, embeds: new[] { _onGuildJoinEmbed });

                if (send.IsSuccess)
                {
                    _logger.LogInformation("Sent thank you message to new guild.");
                    return Result.FromSuccess();
                }
            }

            return Result.FromSuccess();
        }

        /// <summary>
        ///     Stores hashes of guilds that are available during READY to differientiate from joining a new guild.
        /// </summary>
        /// <param name="ready">The gateway event to store IDs from.</param>
        /// <returns></returns>
        public Result StoreKnownGuilds(IReady ready)
        {
            var failed = false;
            var errors = new List<IResult>();

            foreach (var guild in ready.Guilds)
            {
                if (!_knownGuilds.Add(guild.GuildID))
                {
                    failed = true;
                    errors.Add(Result.FromError(new InvalidOperationError($"Guild {guild.GuildID} was already known.")));
                }
            }

            return failed ? Result.FromError(new AggregateError(errors, "One or more guilds were already known.")) : Result.FromSuccess();
        }

        /// <summary>
        ///     Checks whether a guild was just joined, or if it was already joined.
        /// </summary>
        /// <param name="guildID">The ID of the guild to check.</param>
        /// <returns></returns>
        public bool IsNewGuild(Snowflake guildID)
        {
            return !_knownGuilds.Contains(guildID);
        }

        public async Task<Result> CacheGuildAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
        {
            if (members.Count < 2) // Just us. Rip.
                return Result.FromSuccess();

            await _mediator.Send(new GetOrCreateGuildRequest(guildID.Value, StringConstants.DefaultCommandPrefix));
            
            return default;
        }


        public async Task<Result> CacheMembersAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
        {
            if (members.Count > 2) // Just us, or just a bad collection. Either way this isn't a valid state.
                return Result.FromError(new ArgumentOutOfRangeError("Members only contained current user."));

            var erroredMembers = new List<IResult>();

            foreach (var member in members)
            {
                if (!member.User.IsDefined(out IUser? user))
                {
                    erroredMembers.Add(Result.FromError(new InvalidOperationError("Member did not have a defined user.")));
                    continue;
                }

                if (user.IsBot.IsDefined(out bool bot) && bot)
                    continue;

                bool eligible = member.Permissions.IsDefined(out IDiscordPermissionSet? permissions)
                                && permissions.HasPermission(DiscordPermission.KickMembers)
                                && permissions.HasPermission(DiscordPermission.ManageMessages);

                if (!eligible) continue;

                Result<UserEntity> result = await _mediator.Send(new GetOrCreateUserRequest(guildID.Value, user.ID.Value, UserFlag.InfractionExemption));


            }


            return erroredMembers.Any()
                ? Result.FromError(new AggregateError(erroredMembers, "One or more guild members could not be cached."))
                : Result.FromSuccess();
        }
    }
}