using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remora.Discord.API;
using Remora.Rest.Core;
using DiscordConstants = Remora.Discord.API.Constants;

namespace Silk.Data;

// This was provided by Foxtrek_64#3858 on Discord.a

/// <summary>
/// Converts a Snowflake unto a ulong and back.
/// </summary>
public sealed class SnowflakeConverter : ValueConverter<Snowflake, ulong>
{
    internal static readonly ConverterMappingHints _defaultHints = new(precision: 20, scale: 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="SnowflakeConverter"/> class.
    /// </summary>
    public SnowflakeConverter()
        : base(sf => sf.Value, value => DiscordSnowflake.New(value), _defaultHints)
    { }
}

public sealed class SnowflakeArrayConverter : ValueConverter<Snowflake[], ulong[]>
{
    public SnowflakeArrayConverter() 
        : base(sf => sf.Select(x => x.Value).ToArray(), sf => sf.Select(DiscordSnowflake.New).ToArray(), SnowflakeConverter._defaultHints) { }
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
                   ? DiscordSnowflake.New(value.Value) 
                   : default, _defaultHints)
    { }
}