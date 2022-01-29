#pragma warning disable CA1822 // Mark members as static

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Results;
using Silk.Utilities.HelpFormatter;

#pragma warning disable 1591

namespace Silk.Commands.Bot;

[HelpCategory(Categories.Bot)]
public class BotStatCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IRestHttpClient        _restClient;
    private readonly IDiscordRestChannelAPI _channelApi;

    private readonly DiscordGatewayClient        _gateway;
    private readonly DiscordGatewayClientOptions _gatewayOptions;
    
    public BotStatCommand(
        ICommandContext                       context,
        IRestHttpClient                       restClient,
        IDiscordRestChannelAPI                channelApi,
        DiscordGatewayClient                  gateway, 
        IOptions<DiscordGatewayClientOptions> gatewayOptions)
    {
        _context             = context;
        _restClient          = restClient;
        _channelApi          = channelApi;
        _gateway             = gateway;
        _gatewayOptions = gatewayOptions.Value;
    }

    [Command("botstats", "bs", "botinfo")]
    [Description("Get the current stats for Silk")]
    public async Task<Result> GetBotStatsAsync()
    {
        using var process  = Process.GetCurrentProcess();
        
        var guildsResult = await _restClient
           .GetAsync<IReadOnlyList<IPartialGuild>>("users/@me/guilds",
                                                   b => b
                                                       .AddQueryParameter("with_counts", "true")
                                                       .WithRateLimitContext());
        
        if (!guildsResult.IsDefined(out var guilds))
            return Result.FromError(guildsResult.Error!);
        
        int guildCount  = guilds.Count;
        int memberCount = guilds.Aggregate(0, (current, guild) => current + (guild.ApproximateMemberCount.IsDefined(out var count) ? count : 0));
        
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);

        var heapMemory = $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB";
        
        var embed = new Embed()
        {
            Title  = $"Stats (Shard {(_gatewayOptions.ShardIdentification is {} si ? si.ShardID + 1 : 1)}):",
            Colour = Color.Gold,
            Fields = new EmbedField[]
            {
                new("Latency:", $"{_gateway.Latency.TotalMilliseconds:n0} ms", true),
                new("Guilds:", guildCount.ToString(), true),
                new("Members:", memberCount.ToString(), true),
                new("Memory:", heapMemory, true),
                new("Threads:", $"{ThreadPool.ThreadCount}", true),
                new("Uptime:", $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)}", true)
            }
        };
        
        var res = await _channelApi.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res.Error);
    }
}