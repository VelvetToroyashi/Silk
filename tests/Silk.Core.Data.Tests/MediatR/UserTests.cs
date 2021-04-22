using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.Tests.MediatR
{
    public class UserTests
    {
        private const ulong UserId = 1234567890;
        private const ulong GuildId = 10;
        private const string ConnectionString = "Server=localhost; Port=5432; Database=unit_test; Username=silk; Password=silk; Include Error Detail=true;";
        private readonly Checkpoint _checkpoint = new() {TablesToIgnore = new[] {"Guilds", "__EFMigrationsHistory"}, DbAdapter = DbAdapter.Postgres};
        private readonly IServiceCollection _provider = new ServiceCollection();

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
            _context.Guilds.Add(new() {Id = GuildId});
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
            User? result;

            //Act
            await _mediator.Send(new AddUserRequest(GuildId, UserId));
            result = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId && u.GuildId == GuildId);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task MediatR_Add_Throws_When_User_Exists()
        {
            //Arrange
            var request = new AddUserRequest(GuildId, UserId);
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
            User? user;
            //Act
            user = await _mediator.Send(new GetUserRequest(GuildId, UserId));
            //Assert
            Assert.IsNull(user);
        }

        [Test]
        public async Task MediatR_Get_Returns_NonNull_When_User_Exists()
        {
            //Arrange
            User? user;
            await _mediator.Send(new AddUserRequest(GuildId, UserId));
            //Act
            user = await _mediator.Send(new GetUserRequest(GuildId, UserId));
            //Assert
            Assert.IsNotNull(user);
        }

        [Test]
        public async Task MediatR_Update_Returns_Updated_User()
        {
            //Arrange
            User before;
            User after;
            before = await _mediator.Send(new AddUserRequest(GuildId, UserId));
            //Act
            after = await _mediator.Send(new UpdateUserRequest(GuildId, UserId, UserFlag.Staff));
            //Assert
            Assert.AreNotEqual(before, after);
        }

        [Test]
        public async Task MediatR_Update_Throws_When_User_Does_Not_Exist()
        {
            //Arrange
            AsyncTestDelegate send;
            //Act
            send = async () => await _mediator.Send(new UpdateUserRequest(GuildId, UserId));
            //Assert
            Assert.ThrowsAsync<InvalidOperationException>(send);
        }

        [Test]
        public async Task MediatR_GetOrCreate_Creates_When_User_Does_Not_Exist()
        {
            //Arrange
            User? user;
            //Act
            await _mediator.Send(new GetOrCreateUserRequest(GuildId, UserId));
            user = await _mediator.Send(new GetUserRequest(GuildId, UserId));
            //Assert
            Assert.IsNotNull(user);
        }

        [Test]
        public async Task MediatR_GetOrCreate_Returns_User_When_User_Exists()
        {
            //Arrange
            User? user;
            await _mediator.Send(new AddUserRequest(GuildId, UserId));
            //Act
            user = await _mediator.Send(new GetOrCreateUserRequest(GuildId, UserId));
            //Assert
            Assert.IsNotNull(user);
        }
    }
}