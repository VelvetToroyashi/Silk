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
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;

namespace Silk.Data.Tests.MediatR;

public class UserTests
{
    private readonly Snowflake              UserId           = new(1234567890);
    private readonly    Snowflake              GuildId          = new(10);
    private const    string             ConnectionString = "Server=localhost; Port=5432; Database=unit_test; Username=silk; Password=silk; Include Error Detail=true;";
    private readonly Checkpoint         _checkpoint      = new() { TablesToIgnore = new[] { "Guilds", "__EFMigrationsHistory" }, DbAdapter = DbAdapter.Postgres };
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
    public async Task MediatR_Add_Inserts_When_User_Does_Not_Exist()
    {
        // Arrange
        UserEntity? result;

        //Act
        await _mediator.Send(new AddUser.Request(GuildId, UserId));
        result = await _context.Users.FirstOrDefaultAsync(u => u.ID == UserId && u.GuildID == GuildId);

        //Assert
        Assert.IsNotNull(result);
    }

    [Test]
    public async Task MediatR_Add_Throws_When_User_Exists()
    {
        //Arrange
        var               request = new AddUser.Request(GuildId, UserId);
        AsyncTestDelegate send;
        //Act
        await _mediator.Send(request);
        send = async () => await _mediator.Send(request);
        //Assert
        Assert.ThrowsAsync<DbUpdateException>(send);
    }

    [Test]
    public async Task MediatR_Get_Returns_Null_When_User_Does_Not_Exist()
    {
        //Arrange
        UserEntity? user;
        //Act
        user = await _mediator.Send(new GetUser.Request(GuildId, UserId));
        //Assert
        Assert.IsNull(user);
    }

    [Test]
    public async Task MediatR_Get_Returns_NonNull_When_User_Exists()
    {
        //Arrange
        UserEntity? user;
        await _mediator.Send(new AddUser.Request(GuildId, UserId));
        //Act
        user = await _mediator.Send(new GetUser.Request(GuildId, UserId));
        //Assert
        Assert.IsNotNull(user);
    }

    [Test]
    public async Task MediatR_Update_Returns_Updated_User()
    {
        //Arrange
        UserEntity before;
        UserEntity after;
        before = await _mediator.Send(new AddUser.Request(GuildId, UserId));
        //Act
        after = await _mediator.Send(new UpdateUser.Request(GuildId, UserId, UserFlag.WarnedPrior));
        //Assert
        Assert.AreNotEqual(before, after);
    }

    [Test]
    public async Task MediatR_Update_Throws_When_User_Does_Not_Exist()
    {
        //Arrange
        AsyncTestDelegate send;
        //Act
        send = async () => await _mediator.Send(new UpdateUser.Request(GuildId, UserId));
        //Assert
        Assert.ThrowsAsync<InvalidOperationException>(send);
    }

    [Test]
    public async Task MediatR_GetOrCreate_Creates_When_User_Does_Not_Exist()
    {
        //Arrange
        UserEntity? user;
        //Act
        await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        user = await _mediator.Send(new GetUser.Request(GuildId, UserId));
        //Assert
        Assert.IsNotNull(user);
    }

    [Test]
    public async Task MediatR_GetOrCreate_Returns_User_When_User_Exists()
    {
        //Arrange
        Result<UserEntity> user;
        await _mediator.Send(new AddUser.Request(GuildId, UserId));
        //Act
        user = await _mediator.Send(new GetOrCreateUser.Request(GuildId, UserId));
        //Assert
        Assert.IsTrue(user.IsSuccess);
    }
}