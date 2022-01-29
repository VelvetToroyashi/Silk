using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway;
using Remora.Results;
using Silk.Data;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.General;

[HelpCategory(Categories.Misc)]
public class PingCommand : CommandGroup
{
    private readonly GuildContext         _db;
    private readonly ICommandContext      _context;
    private readonly DiscordGatewayClient _gateway;
    
    private readonly IDiscordRestChannelAPI _channels;

    public PingCommand
    (
        GuildContext db,
        ICommandContext context,
        DiscordGatewayClient gateway,
        IDiscordRestChannelAPI channels
    )
    {
        _db       = db;
        _context  = context;
        _gateway  = gateway;
        _channels = channels;
    }

    [Command("ping")]
    [Description("Pong! This is the latency of Silk!; if something seems slow, it's probably because of high latency!")]
    public async Task<Result<IMessage>> Ping()
    {
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
                new("→ Discord API Latency ←", "```cs\n" + "Fetching..".PadLeft(15, '⠀') + "```", true)
            }
        };

        var message = await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });

        if (!message.IsSuccess)
            return message;

        var sw = Stopwatch.StartNew();
        
        var typing = await _channels.TriggerTypingIndicatorAsync(_context.ChannelID);
        
        sw.Stop();
        
        if (!typing.IsSuccess)
            return Result<IMessage>.FromError(typing.Error);
        
        var apiLat = sw.ElapsedMilliseconds.ToString("N0");

        embed = embed with
        {
            Fields = new[]
            {
                (embed.Fields.Value[0] as EmbedField)! with { Value = $"```cs\n" + $"{(message.Entity.Timestamp - (_context as MessageContext)!.MessageID.Timestamp).TotalMilliseconds:N0} ms".PadLeft(10) + "```" },
                (embed.Fields.Value[1] as EmbedField)!,
                (embed.Fields.Value[2] as EmbedField)!,
                (embed.Fields.Value[3] as EmbedField)! with { Value = $"```cs\n" + $"{GetDbLatency()}".PadLeft(7) + " ms```" },
                (embed.Fields.Value[4] as EmbedField)!,
                (embed.Fields.Value[5] as EmbedField)! with { Value = $"```cs\n" + $"{apiLat} ms".PadLeft(11) + "```" },
            }
        };
        
        return await _channels.EditMessageAsync(_context.ChannelID, message.Entity.ID, embeds: new[] { embed });
    }

    private int GetDbLatency()
    {
        var sw = Stopwatch.StartNew();
        _db.Database.ExecuteSqlRaw("SELECT first_value(\"Id\") OVER () FROM \"guilds\"");
        sw.Stop();
        return (int)sw.ElapsedMilliseconds;
    }
}