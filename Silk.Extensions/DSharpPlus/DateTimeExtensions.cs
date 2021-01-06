#region

using System;

#endregion

namespace Silk.Extensions.DSharpPlus
{
    public static class DateTimeExtensions
    {
        public static TimeSpan GetTime(this DateTime dt)
        {
            return dt - DateTime.Now;
        }
    }
}