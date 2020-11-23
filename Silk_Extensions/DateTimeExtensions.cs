using System;

namespace SilkBot.Extensions
{
    public static class DateTimeExtensions
    {
        public static TimeSpan GetTime(this DateTime dt)
        {
            return dt - DateTime.Now;
        }
    }
}