using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using NodaTime;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.MediatR.Users;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Miscellaneous;

[Category(Categories.Misc)]
public class TimezoneCommand : CommandGroup
{
    private readonly IMediator              _mediator;
    private readonly ICommandContext        _context;
    private readonly IDateTimeZoneProvider  _timezones;
    private readonly IDiscordRestChannelAPI _channels;
    
    
    public TimezoneCommand(IMediator mediator, ICommandContext context, IDateTimeZoneProvider timezones, IDiscordRestChannelAPI channels)
    {
        _mediator  = mediator;
        _context   = context;
        _channels  = channels;
        _timezones = timezones;
    }

    [Command("timezone", "tz")]
    [Description("Set your timezone! Useful for reminders!")]
    public async Task<IResult> SetTimezoneAsync
    (
        [Description("Your timezone, e.g. America/New_York.\n" +
                     "Feel free to use https://kevinnovak.github.io/Time-Zone-Picker to find out the proper formatting for your timezone!")]
        string timezoneCode,
        
        [Option("public")]
        [Description("Whether your timezone will be publically accessible.")]
        bool? shared = null
    )
    {
        var dto = _timezones.GetZoneOrNull(timezoneCode);

        if (dto is not { } zone)
            return await _channels.CreateMessageAsync(_context.ChannelID, "I don't know what timezone that is!");

        var time = zone.GetUtcOffset(Instant.FromDateTimeOffset(DateTimeOffset.UtcNow));

        await _mediator.Send(new SetUserTimezone.Request(_context.User.ID, timezoneCode, shared));
        
        return await _channels.CreateMessageAsync(_context.ChannelID, $"Done! The current time should be **{DateTimeOffset.UtcNow + TimeSpan.FromSeconds(time.Seconds)}**.");
    }
}