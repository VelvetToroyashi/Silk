using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Remora.Rest.Core;
using Respawn;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;

namespace Silk.Data.Tests.MediatR;

public class GuildConfigTests
{
    private readonly    Snowflake              GuildId          = new(10);
    private const    string             ConnectionString = "Server=localhost; Port=5432; Database=unit_test; Username=silk; Password=silk; Include Error Detail=true;";
    private readonly Checkpoint         _checkpoint      = new() { TablesToIgnore = new[] { "__EFMigrationsHistory" }, DbAdapter = DbAdapter.Postgres };
    private readonly IServiceCollection _provider        = new ServiceCollection();

    private GuildContext _context;

    private IMediator _mediator;

    [OneTimeSetUp]
    public async Task GlobalSetUp()
    {
        _provider.AddDbContext<GuildContext>(o => o.UseNpgsql(ConnectionString), ServiceLifetime.Transient);
        _provider.AddMediatR(typeof(GuildContext));
        _mediator = _provider.BuildServiceProvider().GetRequiredService<IMediator>();

        _context = _provider.BuildServiceProvider().GetRequiredService<GuildContext>();
        _context.Database.Migrate();
    }

    [OneTimeTearDown]
    public async Task Cleanup()
    {
        _context.Guilds.RemoveRange(_context.Guilds);
        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _checkpoint.Reset(connection);
    }

    [Test]
    public async Task GetReturnsNullForNonGuild()
    {
        //Arrange
        GuildConfigEntity? result;
        //Act
        result = await _mediator.Send(new GetGuildConfig.Request(GuildId));
        //Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task ReturnsConfigWhenGuildExists()
    {
        //Arrange
        GuildConfigEntity? result;
        await _mediator.Send(new GetOrCreateGuild.Request(GuildId, ""));
        //Act
        result = await _mediator.Send(new GetGuildConfig.Request(GuildId));
        //Assert
        Assert.IsNotNull(result, "Config is null!");
    }
}