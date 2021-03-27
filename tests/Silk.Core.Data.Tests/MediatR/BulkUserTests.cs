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
        private const string ConnectionString = "Server=localhost; Port=5432; Database=silk; Username=silk; Password=silk; Include Error Detail=true;";

        private IMediator _mediator;
        private readonly IServiceCollection _provider = new ServiceCollection();
        private readonly Checkpoint _checkpoint = new() {TablesToIgnore = new[] {"Guilds"}, DbAdapter = DbAdapter.Postgres};

        private GuildContext _context;

        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            _provider.AddDbContext<GuildContext>(o => o.UseNpgsql(ConnectionString), ServiceLifetime.Transient);
            _provider.AddMediatR(typeof(GuildContext));
            _mediator = _provider.BuildServiceProvider().GetRequiredService<IMediator>();

            _context = _provider.BuildServiceProvider().GetRequiredService<GuildContext>();
            _context.Guilds.Add(new() {Id = GuildId});
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
        public async Task MediatR_BulkAdd_Inserts_All_Users_When_None_Exist()
        {
            //Arrange
            List<User> users = new[]
            {
                new User() {Id = 1, GuildId = GuildId},
                new User() {Id = 2, GuildId = GuildId},
                new User() {Id = 3, GuildId = GuildId},
            }.ToList();

            List<User> result;
            //Act
            await _mediator.Send(new BulkAddUserRequest(users));
            result = await _context.Users.Where(u => u.GuildId == GuildId).ToListAsync();
            //Assert
            Assert.AreEqual(users.Count, result.Count);
        }

        [Test]
        public async Task MediatR_BulkAdd_Skips_Users_When_User_Exists()
        {
            //Arrange
            await _mediator.Send(new AddUserRequest(GuildId, 1));
            List<User> users = new[]
            {
                new User() {Id = 1, GuildId = GuildId},
                new User() {Id = 2, GuildId = GuildId},
            }.ToList();
            int result;

            //Act
            await _mediator.Send(new BulkAddUserRequest(users));
            result = _context.Users.Count();

            //Assert
            Assert.AreEqual(users.Count, result);
        }

        [Test]
        public async Task MediatR_Bulk_Update_Updates_All_Users()
        {
            //Arrange
            User[] updatedUsers = new User[2];
            List<User> users = new[]
            {
                new User() {Id = 1, GuildId = GuildId},
                new User() {Id = 2, GuildId = GuildId},
            }.ToList();
            users = (await _mediator.Send(new BulkAddUserRequest(users))).ToList();
            //Act
            users.CopyTo(updatedUsers);

            foreach (User u in updatedUsers)
                u.Flags = UserFlag.Staff;

            await _mediator.Send(new BulkUpdateUserRequest(users));
            updatedUsers = _context.Users.ToArray();
            //Assert
            Assert.AreNotEqual(users, updatedUsers);

            foreach (var user in updatedUsers)
                Assert.AreEqual(UserFlag.Staff, user.Flags);
        }
    }
}