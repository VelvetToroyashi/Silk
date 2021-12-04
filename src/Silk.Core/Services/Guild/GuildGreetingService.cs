using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR;
using Silk.Core.Services.Data;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server
{
    public class GuildGreetingService : BackgroundService
    {
        private readonly List<MemberGreetingEntity> _membersToGreet = new();

        private readonly GuildConfigCacheService       _config;
        private readonly ILogger<GuildGreetingService> _logger;
        
        private readonly IMediator                     _mediator;
        
        private readonly IDiscordRestUserAPI           _userApi;
        private readonly IDiscordRestGuildAPI          _guildApi;
        private readonly IDiscordRestChannelAPI        _channelApi;
        
        public GuildGreetingService
        (
            GuildConfigCacheService config,
            ILogger<GuildGreetingService> logger,
            IMediator mediator,
            IDiscordRestUserAPI userApi,
            IDiscordRestGuildAPI guildApi,
            IDiscordRestChannelAPI channelApi
        )
        {
            _config = config;
            _logger = logger;
            _mediator = mediator;
            _userApi = userApi;
            _guildApi = guildApi;
            _channelApi = channelApi;
        }

        /// <summary>
        /// Queues a user on a specific guild to be greeted.
        /// </summary>
        /// <param name="guildID">The ID of the guild the member resides on.</param>
        /// <param name="userID">The ID of the user to greet.</param>
        /// <returns></returns>
        public async Task<Result> QueueGreeting(Snowflake guildID, Snowflake userID)
        {
            if (_membersToGreet.Any(g => g.GuildId == guildID.Value && g.UserId == userID.Value))
                return Result.FromError(new InvalidOperationError($"{userID} is already being greeted on {guildID}."));

            var config = await _config.GetConfigAsync(guildID.Value);
            
            if (!config.Greetings.Any())
                return Result.FromError(new InvalidOperationError($"No greetings are configured for {guildID}."));

            if (config.Greetings.All(p => p.Option is GreetingOption.DoNotGreet))
                return Result.FromError(new InvalidOperationError($"{guildID} does not have any active greetings."));
            
            //There could be multiple greetings, so we check each one.
            foreach (var greeting in config.Greetings)
            {
                if (greeting.Option is GreetingOption.DoNotGreet)
                    continue;

                var greetingResult = await _mediator.Send(new AddMemberGreeting.Request(guildID.Value, userID.Value));
                
                
                _membersToGreet.Add(new()
                {
                    UserId = userID.Value,
                    GuildId = guildID.Value,
                });
            }

            return Result.FromSuccess();
        }
        
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_membersToGreet.Count == 0)
                {
                    await Task.Delay(200, ct);
                    continue;
                }

                for (int i = _membersToGreet.Count; i <= 0; i--)
                {
                    var potentialGreeting = _membersToGreet[i];
                    var config = await _config.GetConfigAsync(potentialGreeting.GuildId);

                    foreach (var greeting in config.Greetings)
                    {
                        if (greeting.Option is GreetingOption.DoNotGreet)
                            continue;

                        var guildResult = await _guildApi.GetGuildAsync(new(potentialGreeting.GuildId), ct: ct);

                        if (!guildResult.IsDefined(out var guild))
                        {
                            _logger.LogError($"Failed to get guild {potentialGreeting.GuildId}.");
                            continue;
                        }
                        
                        var memberResult = await _guildApi.GetGuildMemberAsync(new(greeting.GuildId), new(potentialGreeting.UserId), ct);

                        if (!memberResult.IsDefined(out var member))
                        {
                            _logger.LogError(EventIds.Service, memberResult.Error!.Message);
                            break;
                        }
                        
                        if (greeting.Option is GreetingOption.GreetOnJoin)
                        {
                            var res = await GreetAsync(greeting.GuildId, potentialGreeting.UserId, greeting.ChannelId, greeting.Message);
                            
                            if (!res.IsSuccess)
                                _logger.LogError(EventIds.Service, res.Error.Message);
                            
                            break;
                        }
                        else if (greeting.Option is GreetingOption.GreetOnRole)
                        {
                            if (member.Roles.All(r => r.Value != greeting.MetadataSnowflake))
                                continue;

                            var res = await GreetAsync(greeting.GuildId, potentialGreeting.UserId, greeting.ChannelId, greeting.Message);

                            if (!res.IsSuccess)
                                _logger.LogError(EventIds.Service, res.Error.Message);
                        }
                        else if (greeting.Option is GreetingOption.GreetOnChannelAccess)
                        {
                            if (!greeting.MetadataSnowflake.HasValue)
                            {
                                _logger.LogError(EventIds.Service, "Greeting set to channel access, but no channel was defined for {Guild}.", greeting.GuildId);
                                continue;
                            }
                            
                            var channelResult = await _channelApi.GetChannelAsync(new(greeting.MetadataSnowflake.Value), ct);
                            
                            if (!channelResult.IsDefined(out var channel))
                            {
                                _logger.LogError(EventIds.Service, channelResult.Error!.Message);
                                break;
                            }

                            var permissions = DiscordPermissionSet.ComputePermissions
                                (
                                 new(potentialGreeting.UserId),
                                 guild.Roles.Single(r => r.ID == guild.ID),
                                 guild.Roles.Where(r => member.Roles.Contains(r.ID)).ToArray(),
                                 channel.PermissionOverwrites.Value
                                );

                            if (!permissions.HasPermission(DiscordPermission.SendMessages))
                                continue;
                            
                            var res = await GreetAsync(greeting.GuildId, potentialGreeting.UserId, greeting.ChannelId, greeting.Message);
                            
                            if (!res.IsSuccess)
                                _logger.LogError(EventIds.Service, res.Error.Message);
                        }
                        
                    }
                }
            }
        }
        
        private async Task<Result> GreetAsync(ulong guildID, ulong memberID, ulong channelId, string greetingMessage)
        {
            string formattedMessage;
            
            var memberResult = await _userApi.GetUserAsync(new(memberID));

            if (!memberResult.IsDefined(out var member))
                return Result.FromError(memberResult.Error!);
            
            if (greetingMessage.Contains("{s}"))
            {
                var guildResult = await _guildApi.GetGuildAsync(new(guildID));
                
                if (!guildResult.IsDefined(out var guild))
                    return Result.FromError(guildResult.Error!); //This checks `IsSuccess`, which implies the error isn't null
                
                formattedMessage = greetingMessage.Replace("{s}", guild.Name)
                                                  .Replace("{u}", member.Username)
                                                  .Replace("{@u}", $"<@{member.ID}>");
            }
            else
            {
                formattedMessage = greetingMessage.Replace("{u}", member.Username)
                                                  .Replace("{@u}", $"<@{member.ID}>");
            }

            Result<IMessage> sendResult;

            if (formattedMessage.Length <= 2000)
            {
                sendResult = await _channelApi.CreateMessageAsync(new(channelId), formattedMessage);
            }
            else
            {
                var embed = new Embed(Colour: Color.FromArgb(47, 49, 54));

                sendResult = await _channelApi.CreateMessageAsync(new(channelId), embeds: new[] { embed });
            }

            await _mediator.Send(new DeleteMemberGreeting.Request(guildID, memberID));

            return sendResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(sendResult.Error);
        }

    }
}