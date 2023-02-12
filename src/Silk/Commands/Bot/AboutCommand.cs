using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Rest.Extensions;
using Remora.Plugins;
using Remora.Rest;
using Remora.Results;
using Silk.Extensions;
using Silk.Shared.Constants;
using Silk.Shared.Types;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using StackExchange.Redis;

namespace Silk.Commands.Bot;


[Category(Categories.Bot)]
public class AboutCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestOAuth2API  _oauthApi;
    private readonly ICommandContext        _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly IShardIdentification   _shard;
    private readonly PluginTree             _plugins;
    
    public AboutCommand
    (
        IDiscordRestChannelAPI channelApi,
        IDiscordRestOAuth2API oauthApi,
        ICommandContext context,
        IConnectionMultiplexer redis,
        IShardIdentification shard,
        PluginTree plugins
    )
    {
        _channelApi   = channelApi;
        _oauthApi     = oauthApi;
        _context      = context;
        _redis        = redis;
        _shard        = shard;
        _plugins      = plugins;
    }
    
[Command("about")]
[Description("Shows relevant information, data and links about Silk!")]
public async Task<Result> SendBotInfo()
{
    var appResult = await _oauthApi.GetCurrentBotApplicationInformationAsync();

    if (!appResult.IsDefined(out var app))
        return (Result)appResult;

    Version? remora = typeof(DiscordGatewayClient).Assembly.GetName().Version;

    var db = _redis.GetDatabase();
    var guilds = (string?)await db.StringGetAsync(ShardHelper.GetShardGuildCountStatKey(_shard.ShardID));

    if (guilds is null)
        return Result.FromError(new InvalidOperationError("Could not retrieve guild count from Redis."));

    var infoEmbed = new Embed
    {
        Title  = "About Silk!",
        Colour = Color.Gold,
        Fields = new IEmbedField[]
        {
            new EmbedField("Guild Count:", guilds, true),
            new EmbedField("Owners:", app.Team is null ? app.Owner!.Value.Username.Value : app.Team?.Members.Select(t => t.User.Username.Value).Join(", ") ?? "Unknown", true),
            new EmbedField("Remora Version:", remora?.ToString() ?? "Unknown", true),
            new EmbedField("Silk! Core:", StringConstants.Version, true)
        }
    };
    
    var invite = $"https://discord.com/api/oauth2/authorize?client_id={app.ID}&permissions=1100484045846&scope=bot%20applications.commands";
    
    var res = await _channelApi.CreateMessageAsync
    (
     _context.GetChannelID(),
     embeds: new[] { infoEmbed, GetPluginInfoEmbed() },
     components: new IMessageComponent[]
     {
         new ActionRowComponent(new IMessageComponent[]
         {
             new ButtonComponent(ButtonComponentStyle.Link, "Invite", URL: invite),
             new ButtonComponent(ButtonComponentStyle.Link, "Source", URL: "https://silkbot.cc/src/"),
             new ButtonComponent(ButtonComponentStyle.Link, "Support", URL: StringConstants.SupportInvite)
         })
     });

    return (Result)res;
}

    private IEmbed GetPluginInfoEmbed()
    {
        const int MaxEmbedCharacterLength = 56; // Picked because this is the max length before word-wrap kicks in in portrait mode.
        
        if (_plugins.Branches.Count is 0)
        {
            return new Embed
            {
                Title = "No plugins loaded.",
                Colour = Color.Crimson,
                Footer = new EmbedFooter("No plugins were loaded with Silk!, or they failed to load.       \u200b")
            };
        }

        var plugins = _plugins
                     .Branches
                     .Select(p => new EmbedField
                                 (
                                  $"{p.Plugin.Name} Version {p.Plugin.Version.ToString(3)}", 
                                  p.Plugin.Description?.Truncate(MaxEmbedCharacterLength, "[...]") ?? "No description provided.".PadRight(MaxEmbedCharacterLength) + '\u200b', 
                                  false
                                  )
                             );
        
        return new Embed
        {
            Title = "Loaded Plugins:",
            Colour = Color.Gold,
            Fields = plugins.ToArray()
        };
    }
}