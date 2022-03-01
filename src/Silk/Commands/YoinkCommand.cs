using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using SkiaSharp;

namespace Silk.Commands;


public class YoinkCommand : CommandGroup
{
    private readonly HttpClient      _http;
    private readonly ICommandContext _context;
    private readonly IRestHttpClient _rest;
    private readonly IDiscordRestChannelAPI _channels;
    
    public YoinkCommand
    (
        HttpClient             http,
        ICommandContext        context,
        IRestHttpClient        rest,
        IDiscordRestChannelAPI channels
    )
    {
        _http     = http;
        _context  = context;
        _rest     = rest;
        _channels = channels;
    }

    [Command("yoink")]
    public async Task<IResult> YoinkAsync
    (
        [Option("names")] 
        [Description("The name of the emojis.")] 
        string[] names,

        [Option("emojis")]
        [Description("The emojis to use.")]
        IPartialEmoji[] emojis
    )
    {
        if (names.Length < emojis.Length)
            return await _channels.CreateMessageAsync(_context.ChannelID, $"I count {emojis.Length} emojis, but you only gave me {names.Length} names.");

        var created = new List<Snowflake>();
        
        foreach ((var name, var emoji) in names.Zip(emojis))
        {
            var cdnUrl = $"https://cdn.discordapp.com/emojis/{emoji.ID.Value}.{(emoji.IsAnimated.Value ? "gif" : "webp")}?size=256&quality=lossless";
            
            var emojiRes = await _http.GetByteArrayAsync(cdnUrl);
            
            var createResult = await _rest
               .PostAsync<IEmoji>
                   (
                    $"guilds/{_context.GuildID.Value}/emojis",
                    b =>
                    {
                        b
                           .WithRateLimitContext()
                           .WithJson
                                (
                                 json =>
                                 {
                                     json.WriteString("name", name);
                                     json.WriteString("image", $"data:image/{(emoji.IsAnimated.Value ? "gif" : "webp")};base64," + Convert.ToBase64String(emojiRes));
                                 }
                                );
                    });
            
            if (!createResult.IsSuccess)
                return await _channels.CreateMessageAsync(_context.ChannelID, $"Failed to create emoji {name}.");
            
            created.Add(createResult.Entity.ID.Value);
        }
        
        return await _channels.CreateMessageAsync(_context.ChannelID, $"Created {created.Select(c => $"<:_:{c}>").Join(" ")}");
    }
}