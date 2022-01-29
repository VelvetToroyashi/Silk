using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remora.Rest.Core;
using DiscordConstants = Remora.Discord.API.Constants;

namespace Silk.Data;

// This was provided by Foxtrek_64#3858 on Discord.a

/// <summary>
/// Converts a Snowflake unto a ulong and back.
/// </summary>
public sealed class SnowflakeConverter : ValueConverter<Snowflake, ulong>
{
    private static readonly ConverterMappingHints _defaultHints = new(precision: 20, scale: 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="SnowflakeConverter"/> class.
    /// </summary>
    public SnowflakeConverter()
        : base(sf => sf.Value, value => new(value, DiscordConstants.DiscordEpoch), _defaultHints)
    { }
}

public sealed class NullableSnowflakeConverter : ValueConverter<Snowflake?, ulong?>
{
    private static readonly ConverterMappingHints _defaultHints = new(precision: 20, scale: 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="SnowflakeConverter"/> class.
    /// </summary>
    public NullableSnowflakeConverter()
        : base(
               sf => sf.HasValue 
                   ? sf.Value.Value 
                   : default, 
               value => value.HasValue 
                   ? new Snowflake(value.Value, DiscordConstants.DiscordEpoch) 
                   : default, _defaultHints)
    { }
}