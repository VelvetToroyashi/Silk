using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;

namespace Silk.Utilities;

public static class TimeHelper
{
    public static DateTimeOffset? Extract(string? input) => ExtractImpl(input);

    public static DateTimeOffset? DetectTime(this string? input) => Extract(input);

    private static DateTimeOffset? ExtractImpl(string? input)
    {
        if (input is null)
            return null;

        var basicParseResult = MicroTimeParser.TryParse(input);

        if (basicParseResult is not null)
            return DateTimeOffset.UtcNow + basicParseResult;
        
        var parsedTimes = DateTimeV2Recognizer.RecognizeDateTimes(input, CultureInfo.InvariantCulture.DisplayName, DateTime.UtcNow);

        if (parsedTimes.FirstOrDefault() is not { } parsedTime || !parsedTime.Resolution.Values.Any())
            return null;
        
        var currentYear = DateTime.UtcNow.Year;

        var timeModel = parsedTime
                       .Resolution
                       .Values
                       .Where(v => v is DateTimeV2Date or DateTimeV2DateTime)
                       .FirstOrDefault
                            (
                             v => v is DateTimeV2Date dtd
                                 ? dtd.Value.Year                        >= currentYear
                                 : (v as DateTimeV2DateTime)!.Value.Year >= currentYear
                            );
        
        return (timeModel as DateTimeV2Date)?.Value ?? (timeModel as DateTimeV2DateTime)?.Value;
    }
    
    private static class MicroTimeParser
    {
        private static readonly Regex _timeRegex = new(@"(?<quantity>\d+)(?<unit>mo|[ywdhms])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static TimeSpan? TryParse(string input)
        {
            var start = TimeSpan.Zero;
        
            var matches = _timeRegex.Matches(input);

            if (!matches.Any())
                return null;

            var returnResult = matches.Aggregate(start, (c, n) =>
            {
                var multiplier = int.Parse(n.Groups["quantity"].Value);
                var unit       = n.Groups["unit"].Value;

                return c + unit switch
                {
                    "mo" => TimeSpan.FromDays(30  * multiplier),
                    "y"  => TimeSpan.FromDays(365 * multiplier),
                    "w"  => TimeSpan.FromDays(7   * multiplier),
                    "d"  => TimeSpan.FromDays(multiplier),
                    "h"  => TimeSpan.FromHours(multiplier),
                    "m"  => TimeSpan.FromMinutes(multiplier),
                    "s"  => TimeSpan.FromSeconds(multiplier),
                    _    => TimeSpan.Zero
                };
            });

            if (returnResult == TimeSpan.Zero)
                return null;
        
            return returnResult;
        }
    }
}