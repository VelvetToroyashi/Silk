using System;
using System.Globalization;

namespace Silk.Dashboard.Models
{
    public record DiscordOAuthToken(string AccessToken, string RefreshToken, DateTime? AccessTokenExpiration)
    {
        public static DateTime? GetAccessTokenExpiration(string timestamp)
        {
            DateTime? accessTokenExpiration = null;

            if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var convertedDateTime))
            {
                accessTokenExpiration = convertedDateTime;
            }

            return accessTokenExpiration;
        }
    }
}