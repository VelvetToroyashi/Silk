using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Silk.Core.Data.MediatR.Unified.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.Tests.MediatR
{
    public class BulkUserTests
    {
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
        public async Task MediatR_BulkAdd_Inserts_All_Users_When_None_Exist()
        {
            //Arrange
            List<User> users = new()
            {
                new() {Id = 1, GuildId = GuildId},
                new() {Id = 2, GuildId = GuildId}
            };

            int result;
            //Act
            await _mediator.Send(new BulkAddUserRequest(users));
            result = _context.Users.Count();
            //Assert
            Assert.AreEqual(users.Count, result);
        }

        [Test]
        public async Task MediatR_BulkAdd_Skips_Users_When_User_Exists()
        {
            //Arrange
            await _mediator.Send(new AddUserRequest(GuildId, 1));
            List<User> users = new()
            {
                new() {Id = 1, GuildId = GuildId},
                new() {Id = 2, GuildId = GuildId}
            };
            int result;

            //Act
            await _mediator.Send(new BulkAddUserRequest(users));
            result = _context.Users.ToArray().Length;

            //Assert
            Assert.AreEqual(users.Count, result);
        }

        [Test]
        public async Task MediatR_BulkAdd_Takes_Slow_Route_When_Passed_Malformed_Collection()
        {
            //Arrange
            List<User> users = new()
            {
                new() {Id = 1, GuildId = GuildId},
                new() {Id = 2}
            };
            int result;

            //Act
            await _mediator.Send(new BulkAddUserRequest(users));
            result = _context.Users.Count();

            //Assert
            Assert.AreNotEqual(users.Count, result);
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task MediatR_Bulk_Update_Updates_All_Users()
        {
            //Arrange
            User[] updatedUsers = new User[2];
            List<User> users = new()
            {
                new() {Id = 1, GuildId = GuildId},
                new() {Id = 2, GuildId = GuildId}
            };
            users = (await _mediator.Send(new BulkAddUserRequest(users))).ToList();
            //Act
            users.CopyTo(updatedUsers);

            foreach (User u in updatedUsers)
                u.Flags = UserFlag.Staff;

            await _mediator.Send(new BulkUpdateUserRequest(updatedUsers));
            updatedUsers = _context.Users.ToArray();
            //Assert
            Assert.AreNotEqual(users, updatedUsers);

            foreach (var user in updatedUsers)
                Assert.AreEqual(UserFlag.Staff, user.Flags);
        }
    }
}