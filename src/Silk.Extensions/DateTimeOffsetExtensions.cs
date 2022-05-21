using System;

namespace Silk.Extensions;

public enum TimestampFormat : byte
{
    Relative      = (byte)'R',
    ShortDate     = (byte)'d',
    LongDate      = (byte)'D',
    ShortTime     = (byte)'t',
    LongTime      = (byte)'T',
    ShortDateTime = (byte)'f',
    LongDateTime  = (byte)'F',
}

public static class DateTimeOffsetExtensions
{
    public static string ToTimestamp(this DateTimeOffset dto, TimestampFormat format = TimestampFormat.Relative)
        => $"<t:{dto.ToUnixTimeSeconds()}:{(char)format}>";
}