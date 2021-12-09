using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Silk.Shared.Configuration;
using YumeChan.PluginBase;
using YumeChan.PluginBase.Tools.Data;

namespace Silk.Core.Types;

/// <summary>
///     Implementation of <see cref="IDatabaseProvider{TPlugin}" /> that provides support for Postgres (NpgSQL).
/// </summary>
public class DatabaseProvider<T> : IDatabaseProvider<T> where T : Plugin
{
    private readonly IOptions<SilkConfigurationOptions> _options;
    public DatabaseProvider(IOptions<SilkConfigurationOptions> options) => _options = options;

    public virtual IMongoDatabase GetMongoDatabase() => throw new NotSupportedException();
    public virtual void SetMongoDb(string connectionString, string databaseName)
    {
        throw new NotSupportedException();
    }


    public Action<DbContextOptionsBuilder> GetPostgresContextOptionsBuilder()
    {
        return o =>
        {
            SilkConfigurationOptions? options = _options.Value;
            string? dboptions = options.Persistence
                                       .GetConnectionString()
                                       .Replace("Database=silk;", $"Database=Silk.Plugin-{typeof(T).Name.Replace("Plugin", null)};");

            o.UseNpgsql(dboptions);
        };
    }
}