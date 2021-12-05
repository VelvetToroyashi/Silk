using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;

namespace Silk.Core.Services.Server
{
    public class GuildGreetingService
    {
        private readonly GuildConfigCacheService       _config;
        private readonly ILogger<GuildGreetingService> _logger;
        
        private readonly IDiscordRestUserAPI           _userApi;
        private readonly IDiscordRestGuildAPI          _guildApi;
        private readonly IDiscordRestChannelAPI        _channelApi;
        
        public GuildGreetingService
        (
            GuildConfigCacheService       config,
            ILogger<GuildGreetingService> logger,
            IDiscordRestUserAPI           userApi,
            IDiscordRestGuildAPI          guildApi,
            IDiscordRestChannelAPI        channelApi
        )
        {
            _config = config;
            _logger = logger;

            _userApi = userApi;
            _guildApi = guildApi;
            _channelApi = channelApi;
        }

        /// <summary>
        /// Determines whether a member should be greeted on join, caching them otherwise.
        /// </summary>
        /// <param name="guildID">The ID of the guild.</param>
        /// <param name="user">The member that joined.</param>
        /// <param name="option">The option to use when checking if the member should be greeted.</param>
        /// <returns>A result that may or may have not succeeded.</returns>
        public async Task<Result> TryGreetMemberAsync(Snowflake guildID, IUser user, GreetingOption option)
        {
            var memberRes = await _guildApi.GetGuildMemberAsync(guildID, user.ID);
            
            if (!memberRes.IsDefined(out var member))
                return Result.FromError(memberRes.Error!); // This is guaranteed to be an error.
            
            var config = await _config.GetConfigAsync(guildID.Value);
            
            if (!config.Greetings.Any())
                return Result.FromSuccess();

            if (config.Greetings.All(p => p.Option is GreetingOption.DoNotGreet))
                return Result.FromSuccess();
            
            //There could be multiple greetings, so we check each one.
            foreach (var greeting in config.Greetings)
            {
                if (greeting.Option is GreetingOption.DoNotGreet)
                    continue;

                if (greeting.Option is GreetingOption.GreetOnJoin && option is GreetingOption.GreetOnJoin) // If we can greet immediately, don't make a db call.
                {
                    var res = await GreetAsync(guildID.Value, user.ID.Value, greeting.ChannelId, greeting.Message);
                    
                    if (!res.IsSuccess)
                        return res;

                    continue; // There may be multiple greetings, so we continue.
                }

                if (greeting.Option is GreetingOption.GreetOnRole && option is GreetingOption.GreetOnRole)
                {
                    if (member.Roles.Any(r => r.Value == greeting.MetadataSnowflake))
                    {
                        var res = await GreetAsync(guildID.Value, user.ID.Value, greeting.ChannelId, greeting.Message);
                        
                        if (!res.IsSuccess)
                            return res;
                    }
                    continue;
                }

                if (greeting.Option is GreetingOption.GreetOnChannelAccess)
                    break;
                
                _logger.LogError("Unhandled greeting option. {Option}, spawned from guild {Guild}", greeting.Option, guildID);
            }

            return Result.FromSuccess();
        }
        
        /// <summary>
        /// Attempts to greet a user based on whether or not they can access any of the configured greeting channels.
        /// </summary>
        /// <param name="guildID">The ID of the guild the channel resides on.</param>
        /// <param name="before">The channel before, to compare against.</param>
        /// <param name="after">The channel after, to compare against.</param>
        /// <returns>A result that may or may have not succeeded.</returns>
        public async Task<Result> TryGreetAsync(Snowflake guildID, IChannel before, IChannel after)
        {
            if (!before.GuildID.IsDefined() || !after.GuildID.IsDefined())
                return Result.FromSuccess();
            
            if (!before.PermissionOverwrites.IsDefined(out var overwritesBefore) || 
                !after.PermissionOverwrites.IsDefined(out var overwritesAfter))
                return Result.FromSuccess();
            
            var config = await _config.GetConfigAsync(guildID.Value);
            
            if (!config.Greetings.Any())
                return Result.FromError(new InvalidOperationError($"No greetings are configured for {guildID}."));

            var greeting = config.Greetings
                                 .FirstOrDefault(greeting =>
                                                     greeting.Option is not GreetingOption.GreetOnChannelAccess &&
                                                     greeting.MetadataSnowflake != before.ID.Value);
            
            if (greeting is null)
                return Result.FromError(new InvalidOperationError($"No greetings are configured in {guildID} for {before.ID}."));

            var distinctOverwrites = overwritesBefore.Union(overwritesAfter).Distinct();
            
            foreach (var overwrite in distinctOverwrites)
            {
                if (overwrite.Type is not PermissionOverwriteType.Member)
                    continue;
                
                return await GreetAsync(guildID.Value, overwrite.ID.Value, greeting.MetadataSnowflake.Value, greeting.Message);
            }
            
            return Result.FromSuccess();
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

                sendResult = await _channelApi
                   .CreateMessageAsync
                    (
                      channelID: new(channelId),
                      embeds: new[] { embed },
                      allowedMentions: new AllowedMentions()
                        {
                            Parse = new []
                            {
                                MentionType.Users,
                                MentionType.Roles
                            }
                        }
                    );
            }
            
            return sendResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(sendResult.Error);
        }
    }
}