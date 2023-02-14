using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway;
using Remora.Rest;
using Remora.Results;
using Silk.Data;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;


namespace Silk.Commands.General;

[Category(Categories.Misc)]
public class PingCommand : CommandGroup
{
    private readonly IDbContextFactory<GuildContext> _db;
    private readonly ITextCommandContext             _context;
    private readonly DiscordGatewayClient            _gateway;
    private readonly IDiscordRestChannelAPI          _channels;
    private readonly IRestHttpClient                 _client;

    public PingCommand
    (
        IDbContextFactory<GuildContext> db,
        ITextCommandContext                  context,
        DiscordGatewayClient            gateway,
        IDiscordRestChannelAPI          channels,
        IRestHttpClient client
    )
    {
        _db       = db;
        _context  = context;
        _gateway  = gateway;
        _channels = channels;
        _client   = client;
    }

    [Command("ping")]
    [Description("Pong! This is the latency of Silk!; if something seems slow, it's probably because of high latency!")]
    public async Task<Result<IMessage>> Ping()
    {
        var now = DateTimeOffset.UtcNow;
        
        var apiLat = (now - (_context.Message.EditedTimestamp.IsDefined(out var ts) ? ts.Value : _context.GetMessageID().Timestamp)).TotalMilliseconds.ToString("N0");
        
        var embed = new Embed
        {
            Colour = Color.DodgerBlue,
            Fields = new EmbedField[]
            {
                new("→ Message Latency ←", "```cs\n" + "Fetching..".PadLeft(15, '⠀') + "```", true),
                new("​", "​", true),
                new("→ Websocket latency ←", "```cs\n" + $"{_gateway.Latency.TotalMilliseconds:N0} ms".PadLeft(10, '⠀') + "```", true),

                new("→ Database Latency ←", "```cs\n" + "Fetching..".PadLeft(15, '⠀') + "```", true),
                new("​", "​", true),
                new("→ Command Latency ←", "```cs\n" + $"{apiLat} ms".PadLeft(11, '⠀') + "```", true)
            }
        };

        var message = await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });

        if (!message.IsSuccess)
            return message;
        
        var messageLat = message.Entity.Timestamp - (_context.Message.EditedTimestamp.IsDefined(out var edit) ? edit.Value : _context.GetMessageID().Timestamp);
        
        embed = embed with
        {
            Fields = new[]
            {
                (embed.Fields.Value[0] as EmbedField)! with { Value = "```cs\n" + $"{messageLat.TotalMilliseconds:N0} ms".PadLeft(10) + "```" },
                (embed.Fields.Value[1] as EmbedField)!,
                (embed.Fields.Value[2] as EmbedField)!,
                (embed.Fields.Value[3] as EmbedField)! with { Value = "```cs\n" + $"{GetDbLatency():F2}".PadLeft(7) + " ms```" },
                (embed.Fields.Value[4] as EmbedField)!,
                (embed.Fields.Value[5] as EmbedField)!,
            }
        };
        
        return await _channels.EditMessageAsync(_context.GetChannelID(), message.Entity.ID, embeds: new[] { embed });
    }

    private double GetDbLatency()
    {
        using var db = _db.CreateDbContext();
        var       sw = Stopwatch.StartNew();
        db.Database.ExecuteSqlRaw("SELECT first_value(\"id\") OVER () FROM \"guilds\"");
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }
}