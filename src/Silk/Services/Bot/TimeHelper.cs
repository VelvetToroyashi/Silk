using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Mediator;
using NodaTime;
using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Commands.General;
using Silk.Data.MediatR.Users;

namespace Silk.Services.Bot;

public sealed class TimeHelper
{
    private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                  "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";
    
    private readonly IMediator             _mediator;
    private readonly IDateTimeZoneProvider _timezones;

    private static readonly TimeSpan _buffer = TimeSpan.FromSeconds(2);
    
    public TimeHelper(IMediator mediator, IDateTimeZoneProvider timezones)
    {
        _mediator  = mediator;
        _timezones = timezones;
    }

    public Result<TimeSpan> ExtractTime(string input, Offset? offset, out string remainder)
    {
        remainder = input;
        
        if (string.IsNullOrEmpty(input))
            throw new InvalidOperationException("Cannot extract time from empty string.");

        if (MicroTimeParser.TryParse(input.Split(' ')[0]).IsDefined(out var basicTime))
            return Result<TimeSpan>.FromSuccess(basicTime);
        
        var refTime     = DateTime.UtcNow + (offset?.ToTimeSpan() ?? TimeSpan.Zero);
        var parsedTimes = DateTimeV2Recognizer.RecognizeDateTimes(input, CultureInfo.InvariantCulture.DisplayName, refTime);

        if (parsedTimes.FirstOrDefault() is not { } parsedTime || !parsedTime.Resolution.Values.Any())
            return Result<TimeSpan>.FromError(new NotFoundError(ReminderTimeNotPresent));
        
        var timeModel = parsedTime
                       .Resolution
                       .Values
                       .Where(v => v is DateTimeV2Date or DateTimeV2DateTime)
                       .FirstOrDefault
                        (
                         v => v is DateTimeV2Date dtd 
                         ? dtd.Value > refTime 
                         : (v as DateTimeV2DateTime)!.Value > refTime
                        );

        if (timeModel is null)
            return Result<TimeSpan>.FromError(new NotFoundError(ReminderTimeNotPresent));

        remainder = (input[..parsedTime.Start] + input[(parsedTime.End + 1)..]).Trim();

        return timeModel is DateTimeV2Date vd
            ? (vd.Value - refTime).Add(_buffer)
            : ((timeModel as DateTimeV2DateTime)!.Value - refTime).Add(_buffer);
    }
    
    public async Task<Offset?> GetOffsetForUserAsync(Snowflake userID)
    {
        var user = await _mediator.Send(new GetUser.Request(userID));

        if (user is null || user.TimezoneID is null)
            return null;

        return _timezones.GetZoneOrNull(user.TimezoneID)!.GetUtcOffset(Instant.FromDateTimeOffset(DateTimeOffset.UtcNow));
    }
}