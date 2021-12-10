using System.Collections.Generic;
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
using Silk.Core.Errors;
using Silk.Core.Services.Data;

namespace Silk.Core.Services.Server;

public class GuildGreetingService
{
    private readonly GuildConfigCacheService       _config;
    private readonly ILogger<GuildGreetingService> _logger;

    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestGuildAPI   _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;

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

        _userApi    = userApi;
        _guildApi   = guildApi;
        _channelApi = channelApi;
    }

    /// <summary>
    ///     Determines whether a member should be greeted on join, caching them otherwise.
    /// </summary>
    /// <param name="guildID">The ID of the guild.</param>
    /// <param name="user">The member that joined.</param>
    /// <param name="option">The option to use when checking if the member should be greeted.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    public async Task<Result> TryGreetMemberAsync(Snowflake guildID, IUser user, GreetingOption option)
    {
        Result<IGuildMember> memberRes = await _guildApi.GetGuildMemberAsync(guildID, user.ID);

        if (!memberRes.IsDefined(out IGuildMember? member))
            return Result.FromError(memberRes.Error!); // This is guaranteed to be an error.

        GuildConfigEntity config = await _config.GetConfigAsync(guildID);

        if (!config.Greetings.Any())
            return Result.FromSuccess();

        if (config.Greetings.All(p => p.Option is GreetingOption.DoNotGreet))
            return Result.FromSuccess();

        //There could be multiple greetings, so we check each one.
        foreach (GuildGreetingEntity greeting in config.Greetings)
        {
            if (greeting.Option is GreetingOption.DoNotGreet)
                continue;

            if (greeting.Option is GreetingOption.GreetOnJoin && option is GreetingOption.GreetOnJoin) // If we can greet immediately, don't make a db call.
            {
                Result res = await GreetAsync(guildID, user.ID, greeting.ChannelID, greeting.Message);

                if (!res.IsSuccess)
                    return res;

                continue; // There may be multiple greetings, so we continue.
            }

            if (greeting.Option is GreetingOption.GreetOnRole && option is GreetingOption.GreetOnRole)
            {
                if (member.Roles.Any(r => r == greeting.MetadataID))
                {
                    Result res = await GreetAsync(guildID, user.ID, greeting.ChannelID, greeting.Message);

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
    ///     Attempts to greet a user based on whether or not they can access any of the configured greeting channels.
    /// </summary>
    /// <param name="guildID">The ID of the guild the channel resides on.</param>
    /// <param name="before">The channel before, to compare against.</param>
    /// <param name="after">The channel after, to compare against.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    public async Task<Result> TryGreetAsync(Snowflake guildID, IChannel before, IChannel after)
    {
        if (!before.PermissionOverwrites.IsDefined(out IReadOnlyList<IPermissionOverwrite>? overwritesBefore) ||
            !after.PermissionOverwrites.IsDefined(out IReadOnlyList<IPermissionOverwrite>? overwritesAfter))
            return Result.FromSuccess();

        if (overwritesBefore.Count >= overwritesAfter.Count)
            return Result.FromSuccess();
        
        GuildConfigEntity config = await _config.GetConfigAsync(guildID);

        if (!config.Greetings.Any())
            return Result.FromSuccess();

        GuildGreetingEntity? greeting = config.Greetings
                                              .FirstOrDefault(greeting =>
                                                                  greeting.Option is GreetingOption.GreetOnChannelAccess &&
                                                                  greeting.MetadataID == before.ID);

        if (greeting is null)
            return Result.FromSuccess();

        IEnumerable<IPermissionOverwrite> distinctOverwrites = overwritesAfter.Except(overwritesBefore);

        foreach (IPermissionOverwrite overwrite in distinctOverwrites)
        {
            if (overwrite.Type is not PermissionOverwriteType.Member)
                continue;

            return await GreetAsync(guildID, overwrite.ID, greeting.MetadataID.Value, greeting.Message);
        }

        return Result.FromSuccess();
    }

    private async Task<Result> GreetAsync(Snowflake guildID, Snowflake memberID, Snowflake channelId, string greetingMessage)
    {
        string formattedMessage;

        Result<IUser> memberResult = await _userApi.GetUserAsync(memberID);

        if (!memberResult.IsDefined(out IUser? member))
            return Result.FromError(memberResult.Error!);

        Result permissionRes = await EnsurePermissionsAsync(guildID, channelId);

        if (!permissionRes.IsSuccess)
            return permissionRes;

        if (greetingMessage.Contains("{s}"))
        {
            Result<IGuild> guildResult = await _guildApi.GetGuildAsync(guildID);

            if (!guildResult.IsDefined(out IGuild? guild))
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
            sendResult = await _channelApi.CreateMessageAsync(channelId, formattedMessage);
        }
        else
        {
            var embed = new Embed(Colour: Color.FromArgb(47, 49, 54));

            sendResult = await _channelApi
               .CreateMessageAsync
                    (
                     channelID: channelId,
                     embeds: new[] { embed },
                     allowedMentions: new AllowedMentions
                     {
                         Parse = new[]
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

    /// <summary>
    ///     Determines whether requisite permissions exist for a given channel.
    /// </summary>
    /// <param name="guildID">The ID of the guild the channel exists on.</param>
    /// <param name="channelID">The ID of the channel to check.</param>
    /// <returns>A successful result if the permissions are correct.</returns>
    private async Task<Result> EnsurePermissionsAsync(Snowflake guildID, Snowflake channelID)
    {
        Result<IUser> userResult = await _userApi.GetCurrentUserAsync();

        if (!userResult.IsDefined(out IUser? user))
        {
            _logger.LogCritical("Unable to fetch current user.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch current user."));
        }

        Result<IGuildMember> memberResult = await _guildApi.GetGuildMemberAsync(guildID, user.ID);

        if (!memberResult.IsDefined(out IGuildMember? member))
        {
            _logger.LogCritical("Unable to fetch current user's member.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch current member."));
        }

        Result<IChannel> channelResult = await _channelApi.GetChannelAsync(channelID);

        if (!channelResult.IsDefined(out IChannel? channel))
        {
            _logger.LogCritical("Unable to fetch channel.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch channel."));
        }

        Result<IReadOnlyList<IRole>> rolesResult = await _guildApi.GetGuildRolesAsync(guildID);

        if (!rolesResult.IsDefined(out IReadOnlyList<IRole>? roles))
        {
            _logger.LogCritical("Unable to fetch guild.");
            return Result.FromError(new InvalidOperationError("CRITICAL: Unable to fetch roles."));
        }

        IDiscordPermissionSet permissions = DiscordPermissionSet
           .ComputePermissions
                (
                 user.ID,
                 roles.Single(r => r.ID == guildID),
                 roles.Where(r => member.Roles.Contains(r.ID)).ToArray(),
                 channel.PermissionOverwrites.Value
                );

        if (!permissions.HasPermission(DiscordPermission.SendMessages))
            return Result.FromError(new PermissionError($"I cannot send messages to the specified greeting channel ({channel.ID})."));

        if (!permissions.HasPermission(DiscordPermission.EmbedLinks))
            return Result.FromError(new PermissionError($"I cannot embed links in the specified greeting channel ({channel.ID})."));

        return Result.FromSuccess();
    }
}