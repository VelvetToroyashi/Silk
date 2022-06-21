using Remora.Discord.API;
using Remora.Rest.Core;

namespace Silk.Dashboard.Converters;

public static class MudBlazorSnowflakeConverter
{
    private const string IdValidationError = "Not a valid ID";

    public static readonly NullableSnowflakeConverter    NullableConverter    = new();
    public static readonly NonNullableSnowflakeConverter NonNullableConverter = new();

    public static readonly Func<Snowflake, string> NonNullableValidator =
        s => s != default ? null : IdValidationError;

    public static readonly Func<Snowflake?, string> NullableValidator =
        s => s is not null ? NonNullableValidator((Snowflake) s) : null;

    public class NonNullableSnowflakeConverter : MudBlazor.Converter<Snowflake>
    {
        public NonNullableSnowflakeConverter()
        {
            SetFunc = snowflake => snowflake.Value.ToString();
            GetFunc = ConvertFrom;
        }

        private Snowflake ConvertFrom(string value)
        {
            var r = DiscordSnowflake.TryParse(value, out var snowflake)
                ? snowflake.Value
                : default;
            return r;
        }
    }

    public class NullableSnowflakeConverter : MudBlazor.Converter<Snowflake?>
    {
        public NullableSnowflakeConverter()
        {
            SetFunc = snowflake => snowflake?.Value.ToString();
            GetFunc = ConvertFrom;
        }
        
        private Snowflake? ConvertFrom(string value)
        {
            _ = DiscordSnowflake.TryParse(value, out var snowflake);
            return snowflake;
        }
    }
}