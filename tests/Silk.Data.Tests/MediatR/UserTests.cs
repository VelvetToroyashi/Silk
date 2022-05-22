using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Remora.Rest.Core;
using Remora.Results;
using Respawn;
using Respawn.Graph;
using Silk.Data.DTOs.Guilds.Users;
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;

namespace Silk.Data.Tests.MediatR;

public class UserTests
{
    private readonly Snowflake          UserId           = new(1234567890);
    private readonly Snowflake          GuildId          = new(10);
    private const    string             ConnectionString = "Server=localhost; Port=5432; Database=unit_test; Username=silk; Password=silk; Include Error Detail=true;";
    private readonly Checkpoint         _checkpoint      = new() { TablesToIgnore = new Table[] { "guilds", "__EFMigrationsHistory" }, DbAdapter = DbAdapter.Postgres };
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
        await _context.Database.MigrateAsync();
        _context.Guilds.Add(new() { ID = GuildId });
        await _context.SaveChangesAsync();
    }

    [OneTimeTearDown]
    public async Task Cleanup()
    {
        if (_context.Guilds.Any())
        {
            _context.ChangeTracker.Clear();
            _context.Guilds.RemoveRange(_context.Guilds);
            await _context.SaveChangesAsync();
        }
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
    public async Task GetReturnsNullForNonUser()
    {
        //Arrange
        UserDTO? user;
        //Act
        user = await _mediator.Send(new GetUser.Request(UserId));
        //Assert
        Assert.IsNull(user);
    }

    [Test]
    public async Task GetReturnsUserCorrectly()
    {
        //Arrange
        UserDTO? user;
        await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        //Act
        user = await _mediator.Send(new GetUser.Request(UserId));
        //Assert
        Assert.IsNotNull(user);
    }

    [Test]
    public async Task GetOrCreateCreatesForNonUser()
    {
        //Arrange
        UserDTO? user;
        //Act
        await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        user = await _mediator.Send(new GetUser.Request(UserId));
        //Assert
        Assert.IsNotNull(user);
    }

    [Test]
    public async Task GetOrCreateDoesNotDuplicateUser()
    {
        //Arrange
        Result<UserEntity> user;
        await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        //Act
        user = await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        //Assert
        Assert.IsTrue(user.IsSuccess);
    }
}