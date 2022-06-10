using Remora.Discord.API;
using Remora.Rest.Core;

namespace Silk.Dashboard.Converters;

public static class MudBlazorSnowflakeConverter
{
    private const string IdValidationError = "Not a valid ID";

    public static NullableSnowflakeConverter    NullableConverter    = new();
    public static NonNullableSnowflakeConverter NonNullableConverter = new();

    public static Func<Snowflake, string> NonNullableValidator =
        s => (s != default) ? null : IdValidationError;

    public static Func<Snowflake?, string> NullableValidator =
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
            var r = Snowflake.TryParse(value, out var snowflake, Constants.DiscordEpoch)
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
            _ = Snowflake.TryParse(value, out var snowflake, Constants.DiscordEpoch);
            return snowflake;
        }
    }
}