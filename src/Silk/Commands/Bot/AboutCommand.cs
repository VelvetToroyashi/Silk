using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Commands.Groups;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Results;
using Silk.Extensions;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Bot;


[Category(Categories.Bot)]
public class AboutCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestOAuth2API  _oauthApi;
    private readonly ICommandContext        _context;
    private readonly IRestHttpClient        _restClient;
    private readonly ICacheProvider           _cache;
    
    public AboutCommand(IDiscordRestChannelAPI channelApi, IDiscordRestOAuth2API oauthApi, ICommandContext context, IRestHttpClient restClient, ICacheProvider cache)
    {
        _channelApi = channelApi;
        _oauthApi   = oauthApi;
        _context    = context;
        _restClient = restClient;
        _cache = cache;
    }
    
    [Command("about")]
    [Description("Shows relevant information, data and links about Silk!")]
    public async Task<Result> SendBotInfo()
    {
        var appResult = await _oauthApi.GetCurrentBotApplicationInformationAsync();
        
        if (!appResult.IsDefined(out var app))
            return Result.FromError(appResult.Error!);

        Version? remora = typeof(DiscordGatewayClient).Assembly.GetName().Version;

        var guilds = await _restClient
           .GetAsync<IReadOnlyList<IPartialGuild>>(
                                                   "users/@me/guilds",
                                                   b =>
                                                   {
                                                       b.WithRateLimitContext(_cache);
                                                       b.AddQueryParameter("with_counts", "true");
                                                   });
        
        if (!guilds.IsDefined(out var guildsList))
            return Result.FromError(guilds.Error!);

        IEmbed? embed = new Embed()
        {
            Title  = "About Silk!",
            Colour = Color.Gold,
            Fields = new IEmbedField[]
            {
                new EmbedField("Guild Count:", guildsList.Count.ToString(), true),
                new EmbedField("Owners:", app.Team is null ? app.Owner!.Username.Value : app.Team?.Members.Select(t => t.User.Username.Value).Join(", ") ?? "Unknown", true),
                new EmbedField("Remora Version:", remora?.ToString() ?? "Unknown", true),
                new EmbedField("Silk! Core:", StringConstants.Version, true)
            }
        };
        
        var invite = $"https://discord.com/api/oauth2/authorize?client_id={app.ID}&permissions=1100484045846&scope=bot%20applications.commands";


        var res = await _channelApi
           .CreateMessageAsync(
                               _context.ChannelID,
                               embeds: new[] { embed },
                               components: new IMessageComponent[]
                               {
                                   new ActionRowComponent(new IMessageComponent[]
                                   {
                                       new ButtonComponent(ButtonComponentStyle.Link, "Invite", URL: invite),
                                       new ButtonComponent(ButtonComponentStyle.Link, "Source", URL: "https://velvetthepanda.dev/vtd/Silk"),
                                       new ButtonComponent(ButtonComponentStyle.Link, "Support", URL: "https://discord.gg/HZfZb95"),
                                       new ButtonComponent(ButtonComponentStyle.Link, "Donate", URL: "https://paypal.me/MarshmallowSerg/5")
                                   })
                               });
        
       return res.IsSuccess 
           ? Result.FromSuccess() 
           : Result.FromError(res.Error!);
    }
}