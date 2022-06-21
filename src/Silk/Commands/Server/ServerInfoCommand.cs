using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Users;
using Silk.Extensions;
using Silk.Utilities.HelpFormatter;
using CommandGroup = Remora.Commands.Groups.CommandGroup;

namespace Silk.Commands.Server;

[Category(Categories.Server)]
public class ServerInfoCommand : CommandGroup
{
    private readonly IMediator              _mediator;
    private readonly ICommandContext        _context;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;
    
    
    public ServerInfoCommand(IMediator mediator, ICommandContext context, IDiscordRestGuildAPI guilds, IDiscordRestChannelAPI channels)
    {
        _mediator = mediator;
        _context  = context;
        _guilds   = guilds;
        _channels = channels;
    }

    
    [Command("serverinfo", "si")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Get info about the current Guild")]
    public async Task<IResult> ServerInfo()
    {
        var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value, true);

        if (!guildResult.IsDefined(out var guild))
            return guildResult;
        
        var fields = new List<IEmbedField>();

        fields.Add(new EmbedField("Server Icon:", CDN.GetGuildIconUrl(guild, imageSize: 4096).IsDefined(out var guildIcon) ? $"[Link]({guildIcon})" : "Not Set!", true));
        fields.Add(new EmbedField("Invite Splash:", CDN.GetGuildSplashUrl(guild, imageSize: 4096).IsDefined(out var guildSplash) ? $"[Link]({guildSplash})" : "Not Set!", true));
        fields.Add(new EmbedField("Server Banner:", CDN.GetGuildBannerUrl(guild, imageSize: 4096).IsDefined(out var guildBanner) ? $"[Link]({guildBanner})" : "Not Set!", true));
        
        var memberInformation = $"Max: {(guild.MaxMembers.IsDefined(out var maxMembers) ? $"{maxMembers}" : "Unknown")}\n"                   +
                                $"Current\\*: {(guild.ApproximateMemberCount.IsDefined(out var memberCount) ? $"{memberCount}" : "Unknown")}\n" +
                                $"Online\\*: {(guild.ApproximatePresenceCount.IsDefined(out var onlineCount) ? $"{onlineCount}" : "Unknown")}";

        fields.Add(new EmbedField("Members:" , memberInformation, true));

        var channelsResult = await _guilds.GetGuildChannelsAsync(_context.GuildID.Value); 
        
        if (!channelsResult.IsDefined(out var channels))
        {
            fields.Add(new EmbedField("Channels:", "Channel information is unavailable. Sorry.", true));
        }
        else
        {
            var channelInfo = channels
                             .GroupBy(c => c.Type)
                             .Select(gc => $"{gc.Key.Humanize()}: {gc.Count()}")
                             .Join("\n");
            
            fields.Add(new EmbedField("Channels:", $"{channelInfo}\n Total: {channels.Count}", true));
        }

        var tier = guild.PremiumTier switch
        {
            PremiumTier.None  => ("(No Level)", 100),
            PremiumTier.Tier1 => ("(Level 1)", 200),
            PremiumTier.Tier2 => ("(Level 2)", 300),
            PremiumTier.Tier3 => ("(Level 3)", 500),
            PremiumTier.Tier4 => throw new InvalidOperationException("Tier 4 doesn't exist."),
        };

        fields.Add(new EmbedField("Other Info:", $"Emojis: {guild.Emojis.Count}/{tier.Item2}\n "                                                                        +
                                                 $"Roles: {guild.Roles.Count}\n "                                                                                       +
                                                 $"Boosts: {(guild.PremiumSubscriptionCount.IsDefined(out var boosts) ? $"{boosts}" : "Unknown")} {tier.Item1}\n " +
                                                 $"Progress Bar: {(guild.IsPremiumProgressBarEnabled ? "Yes" : "No")}", true));


        fields.Add(new EmbedField("Server Owner:", $"<@{guild.OwnerID}>", true));
        
        var recent = await _mediator.Send(new GetMostRecentUser.Request(_context.GuildID.Value));
        
        fields.Add(new EmbedField("Most Recent Member:", $"<@{recent?.ID}>", true));
        
        fields.Add(new EmbedField("Server Created:", $"{guild.ID.Timestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({guild.ID.Timestamp.ToTimestamp()})"));

        var features = guild.GuildFeatures.Any() ? guild.GuildFeatures.Select(f => f.Humanize(LetterCasing.Title)).OrderBy(o => o.Length).Join("\n") : "None";
        
        fields.Add(new EmbedField("Features:", features));

        var embed = new Embed
        {
            Title     = $"Information about {guild.Name}:",
            Colour    = Color.Gold,
            Fields    = fields,
            Thumbnail = guildIcon is null ? default(Optional<IEmbedThumbnail>) : new EmbedThumbnail(guildIcon.ToString()),
            Image     = guildBanner is null ? default(Optional<IEmbedImage>) : new EmbedImage(guildBanner.ToString()),
        };
        
        var res = await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });

        return res;
    }
}